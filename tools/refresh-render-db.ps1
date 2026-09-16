<#
.SYNOPSIS
  Refreshes the Render free Postgres database: dumps current data, creates a new
  free Postgres, restores the data, then re-points and redeploys both Render web
  services to the new database.

.DESCRIPTION
  Render free-tier Postgres databases expire 30 days after creation. Run this script
  before each expiry to keep the deployed app running against a fresh database while
  preserving all current data.

  Requires:
    - pg_dump and pg_restore on PATH (the script can install PostgreSQL tools via winget)
    - git on PATH
    - Render API key: passed via -ApiKey or $env:RENDER_API_KEY

.PARAMETER ApiKey
  Render API key. If omitted, falls back to $env:RENDER_API_KEY.

.PARAMETER OwnerId
  Render team/owner id. Defaults to the MYAPP team.

.PARAMETER DatabaseName
  Name of the new Postgres instance to create (must be unique in the owner).

.PARAMETER Keep
  Keep the generated backup file on disk after a successful restore.

.EXAMPLE
  .\tools\refresh-render-db.ps1 -ApiKey rnd_xxxx -DatabaseName iform-db-20261015
#>
[CmdletBinding()]
param(
    [string]$ApiKey,
    [string]$OwnerId = "tea-da0o0aou01pc738pqbv0",
    [string]$DatabaseName = ("iform-db-" + (Get-Date).ToString("yyyyMMdd")),
    [switch]$Keep,
    [switch]$SkipBackup
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version Latest

$BaseUrl = "https://api.render.com/v1"
$ServiceIds = @(
    "srv-da0o2edg1s2s73c2k7vg",
    "srv-da0otndbedkc73b9ca60"
)
$RepoRoot = Split-Path -Parent $PSScriptRoot
$RenderYaml = Join-Path $RepoRoot "render.yaml"
$BackupDir = Join-Path $PSScriptRoot "backups"
$Now = Get-Date
$Timestamp = $Now.ToString("yyyyMMdd-HHmmss")
$BackupFile = Join-Path $BackupDir ("iform-backup-" + $Timestamp + ".dump")
$OldDbId = $null

function Write-Info  { Write-Host ("[INFO ] " + $_) -ForegroundColor Cyan }
function Write-Ok    { Write-Host ("[OK   ] " + $_) -ForegroundColor Green }
function Write-Warn  { Write-Host ("[WARN ] " + $_) -ForegroundColor Yellow }
function Write-Fatal { Write-Host ("[FATAL] " + $_) -ForegroundColor Red; exit 1 }

function Invoke-RenderGet {
    param([string]$Path)
    $h = @{ Authorization = "Bearer " + $ApiKey }
    $resp = Invoke-WebRequest -Uri ($BaseUrl + $Path) -Headers $h -UseBasicParsing
    return $resp.Content | ConvertFrom-Json
}

function Invoke-RenderPost {
    param([string]$Path, [hashtable]$Body = @{})
    $h = @{ Authorization = "Bearer " + $ApiKey; "Content-Type" = "application/json" }
    $resp = Invoke-WebRequest -Uri ($BaseUrl + $Path) -Headers $h -Method Post -Body ($Body | ConvertTo-Json -Depth 6) -UseBasicParsing
    return $resp.Content | ConvertFrom-Json
}

function Invoke-RenderDelete {
    param([string]$Path)
    $h = @{ Authorization = "Bearer " + $ApiKey }
    $resp = Invoke-WebRequest -Uri ($BaseUrl + $Path) -Headers $h -Method Delete -UseBasicParsing
    return $resp
}

function Find-Tool {
    param([string]$Name)
    $found = Get-Command $Name -ErrorAction SilentlyContinue
    if ($found) { return $found.Source }
    $candidates = Get-ChildItem "C:\Program Files\PostgreSQL" -Directory -ErrorAction SilentlyContinue |
        Sort-Object Name -Descending | Select-Object -First 1
    if ($candidates) {
        $candPath = Join-Path $candidates.FullName "bin\$Name.exe"
        if (Test-Path $candPath) { return $candPath }
    }
    return $null
}

function Test-DatabaseReachable {
    param([string]$ConnectionString)
    $url = [Uri]$ConnectionString
    $rhost = $url.Host
    $rport = if ($url.Port -gt 0) { $url.Port } else { 5432 }
    $tcp = New-Object System.Net.Sockets.TcpClient
    try {
        $async = $tcp.BeginConnect($rhost, $rport, $null, $null)
        if (-not $async.AsyncWaitHandle.WaitOne(15000)) { return $false }
        $tcp.EndConnect($async)
        return $true
    } catch { return $false }
    finally { $tcp.Close() }
}

if (-not $ApiKey) { $ApiKey = $env:RENDER_API_KEY }
if (-not $ApiKey) { Write-Host "Render API key required: pass -ApiKey or set RENDER_API_KEY." -ForegroundColor Red; Read-Host "Press Enter to exit"; exit 1 }

if (-not (Get-Command git -ErrorAction SilentlyContinue)) { Write-Fatal "git not found on PATH." }

if ($SkipBackup) {
    Write-Warn "SkipBackup set - data will NOT be preserved; the new DB will rely on DbSeeder."
} else {
    $PgDump = Find-Tool "pg_dump"
    $PgRestore = Find-Tool "pg_restore"
    if (-not $PgDump -or -not $PgRestore) {
        Write-Warn "pg_dump / pg_restore not found."
        $yn = Read-Host "Install PostgreSQL client tools via winget? [Y/n]"
        if ($yn -ne "n" -and $yn -ne "N") {
            winget install -e --id PostgreSQL.PostgreSQL.16 --accept-source-agreements --accept-package-agreements
            $PgDump = Find-Tool "pg_dump"
            $PgRestore = Find-Tool "pg_restore"
        }
        if (-not $PgDump -or -not $PgRestore) {
            Write-Fatal "pg_dump/pg_restore required for backup restore. Re-run after installing PostgreSQL tools (winget install PostgreSQL.PostgreSQL.16) or use -SkipBackup."
        }
    }
    Write-Ok "Using pg_dump: $PgDump"
    Write-Ok "Using pg_restore: $PgRestore"
}

Write-Info "Connecting to Render API..."
$null = Invoke-RenderGet "/postgres?ownerId=$OwnerId"

if (-not (Test-Path $RenderYaml)) { Write-Fatal "render.yaml not found at $RenderYaml." }
$yamlCurrent = Get-Content $RenderYaml -Raw
if ($yamlCurrent -notmatch 'name: (iform-[A-Za-z0-9-]+)') { Write-Fatal "Could not locate current database name in render.yaml." }
$currentDbName = $Matches[1]

Write-Info "Locating current DB '$currentDbName' in Render..."
$allPg = Invoke-RenderGet "/postgres?ownerId=$OwnerId"
$pgList = @($allPg) | ForEach-Object { $_.postgres }
$currentPg = $pgList | Where-Object { $_.name -eq $currentDbName -and $_.role -eq "primary" } | Select-Object -First 1
if ($currentPg) {
    $OldDbId = $currentPg.id
    Write-Ok "Current DB found: $OldDbId (name=$($currentPg.name), status=$($currentPg.status))"
} else {
    $OldDbId = $pgList | Where-Object { $_.status -eq "available" -and $_.role -eq "primary" } | Select-Object -First 1 -ExpandProperty id
    if (-not $OldDbId) { Write-Fatal "No primary Postgres found in owner $OwnerId." }
    Write-Warn "No DB named '$currentDbName'; using available DB $OldDbId instead. You may need to update render.yaml."
}

Write-Info "Fetching connection info for existing DB ($OldDbId)..."
$oldInfo = Invoke-RenderGet "/postgres/$OldDbId/connection-info"
$oldExternal = $oldInfo.externalConnectionString

if (-not $SkipBackup) {
    Write-Info "Testing connectivity to existing DB host..."
    if (-not (Test-DatabaseReachable $oldExternal)) { Write-Fatal "Existing DB host unreachable. Is it suspended? Use -SkipBackup to just create a fresh DB." }
    if (-not (Test-Path $BackupDir)) { New-Item -ItemType Directory -Path $BackupDir -Force | Out-Null }
    Write-Info "Dumping existing database to $BackupFile ..."
    $dumpArgs = @("--dbname=$oldExternal", "--format=custom", "--no-owner", "--no-privileges", "--file=$BackupFile")
    & $PgDump @dumpArgs
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path $BackupFile)) { Write-Fatal "pg_dump failed." }
    Write-Ok "Backup created ($([math]::Round((Get-Item $BackupFile).Length / 1MB, 2)) MB)."
}

Write-Info "Creating new free Postgres '$DatabaseName'..."
$createBody = @{
    name          = $DatabaseName
    ownerId       = $OwnerId
    databaseName  = "iform_quality"
    databaseUser  = "iform_admin"
    plan          = "free"
    region        = "oregon"
    version       = "16"
}
$newDb = Invoke-RenderPost "/postgres" $createBody
$newDbId = $newDb.id
Write-Ok "New DB created: $newDbId (name=$($newDb.name), expires=$($newDb.expiresAt))"

Write-Info "Waiting for $newDbId to become available..."
$deadline = (Get-Date).AddMinutes(10)
$available = $false
while ((Get-Date) -lt $deadline) {
    Start-Sleep -Seconds 15
    $s = Invoke-RenderGet "/postgres/$newDbId"
    if ($s.status -eq "available") { $available = $true; break }
}
if (-not $available) { Write-Fatal "New DB did not become available in time." }
Write-Ok "New DB is available."

Write-Info "Fetching connection info for new DB..."
$newInfo = Invoke-RenderGet "/postgres/$newDbId/connection-info"
$newExternal = $newInfo.externalConnectionString

Write-Info "Waiting for new DB host to accept connections..."
$deadline = (Get-Date).AddMinutes(5)
while (-not (Test-DatabaseReachable $newExternal) -and (Get-Date) -lt $deadline) { Start-Sleep -Seconds 10 }
if (-not (Test-DatabaseReachable $newExternal)) { Write-Fatal "New DB host not reachable." }
Write-Ok "New DB is accepting connections."

if (-not $SkipBackup) {
    Write-Info "Restoring backup into new DB..."
    $restoreArgs = @("--dbname=$newExternal", "--no-owner", "--no-privileges", "--single-transaction", "--file=$BackupFile")
    & $PgRestore @restoreArgs
    if ($LASTEXITCODE -ne 0) { Write-Fatal "pg_restore failed. New DB will still seed from DbSeeder on deploy." }
    Write-Ok "Data restored."
}

Write-Info "Updating render.yaml to point at new DB ($DatabaseName)..."
if (-not (Test-Path $RenderYaml)) { Write-Fatal "render.yaml not found at $RenderYaml." }
$yaml = Get-Content $RenderYaml -Raw
if ($yaml -match 'name: iform-(recovery-db|db-[0-9]+)') {
    $yaml = $yaml -replace 'name: iform-(recovery-db|db-[0-9]+)', ("name: " + $DatabaseName)
}
if ($yaml -notmatch ([regex]::Escape("name: $DatabaseName"))) { Write-Fatal "Could not update render.yaml database name." }
$yaml = $yaml -replace 'databaseName: iform_quality_[a-z0-9]+', ("databaseName: " + $newDb.databaseName)
$yaml = $yaml -replace 'value: "[\d]{4}-[\d]{2}-[\d]{2}(-[\d]+)?"', ('value: "' + (Get-Date).ToString("yyyy-MM-dd") + '"')
Set-Content -Path $RenderYaml -Value $yaml
Write-Ok "render.yaml updated."

Write-Info "Committing and pushing render.yaml to trigger blueprint sync..."
Push-Location $RepoRoot
git add render.yaml
git commit -m "Refresh to new Postgres $DatabaseName (expiry rotation)"
if ($LASTEXITCODE -ne 0) { Pop-Location; Write-Fatal "git commit failed (no changes?)." }
git push origin main
$pushOk = $LASTEXITCODE -eq 0
Pop-Location
if (-not $pushOk) { Write-Fatal "git push failed." }
Write-Ok "Pushed. Two deploys (blueprint_sync) should now be running."

foreach ($sid in $ServiceIds) {
    Write-Info "Waiting for service $sid to go live..."
    $deadline = (Get-Date).AddMinutes(15)
    $live = $false
    $sleepTime = 0
    while ((Get-Date) -lt $deadline) {
        $deploys = Invoke-RenderGet "/services/$sid/deploys?limit=1"
        $latest = @($deploys)[0].deploy
        if ($latest.status -eq "live") { $live = $true; break }
        if ($latest.status -eq "update_failed") { Write-Warn "Latest deploy for $sid failed: $($latest.id)"; break }
        Start-Sleep -Seconds 30
    }
    if ($live) { Write-Ok "Service $sid is LIVE." } else { Write-Warn "Service $sid not yet live - verify manually." }
}

foreach ($sid in $ServiceIds) {
    $svc = Invoke-RenderGet "/services/$sid"
    $url = $svc.serviceDetails.url
    Write-Info "Verifying health at $url/health ..."
    try {
        $h = Invoke-WebRequest -Uri "$url/health" -UseBasicParsing -TimeoutSec 120
        if ($h.StatusCode -eq 200) { Write-Ok "HEALTH OK: $($h.Content)" } else { Write-Warn "Health returned $($h.StatusCode)" }
    } catch { Write-Warn "Health check failed: $($_.Exception.Message)" }
}

Write-Ok "Done. Old DB: $OldDbId | New DB: $newDbId (expires $($newDb.expiresAt)). Verify at https://dashboard.render.com/d/$newDbId"
if (-not $SkipBackup -and $Keep) { Write-Ok "Backup kept at: $BackupFile" }