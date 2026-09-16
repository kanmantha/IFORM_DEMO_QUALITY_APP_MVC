using IFormQualityApp.Data;
using IFormQualityApp.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IFormQualityApp.Services;

/// <summary>
/// Delay thresholds and helpers used by both the UI alerts and the background
/// email service.
/// </summary>
public static class DelayAlertHelper
{
    /// <summary>Alert the manager when a query stays open past this many days.</summary>
    public const int WarningDays = 7;

    /// <summary>Auto-generate an email once a query stays open past this many days.</summary>
    public const int CriticalDays = 30;

    public static int DelayDays(SiteQuery q, DateTime now)
        => q.Status == QueryStatus.Resolved
            ? ((q.ResolvedAt ?? q.RaisedAt).Date - q.RaisedAt.Date).Days
            : (now.Date - q.RaisedAt.Date).Days;

    public static bool IsWarning(int days) => days > WarningDays && days <= CriticalDays;

    public static bool IsCritical(int days) => days > CriticalDays;
}

/// <summary>
/// Periodically scans open queries. Raises a manager alert (audit log) once when
/// a query is delayed over 7 days, and auto-generates a manager email notification
/// once when the delay exceeds 30 days. Emails are rendered and logged to the audit
/// trail so they are visible in-app even without SMTP.
/// </summary>
public class DelayAlertHostedService : BackgroundService
{
    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(6);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DelayAlertHostedService> _logger;

    public DelayAlertHostedService(IServiceScopeFactory scopeFactory, ILogger<DelayAlertHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delay alert processing failed.");
            }

            try
            {
                await Task.Delay(RunInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ProcessAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now = DateTime.UtcNow.Date;

        var openQueries = await db.SiteQueries
            .Include(q => q.Project)
            .Include(q => q.RaisedBy)
            .Where(q => q.Status != QueryStatus.Resolved)
            .ToListAsync(ct);

        var logged = await db.AuditLogs
            .Where(a => a.Action == "DelayAlert" || a.Action == "AutoEmailSent")
            .Select(a => new { a.QueryId, a.Action })
            .AsNoTracking()
            .ToListAsync(ct);

        var managerEmails = await GetManagerEmailsAsync(db, ct);

        foreach (var q in openQueries)
        {
            var days = DelayAlertHelper.DelayDays(q, now);

            if (days > DelayAlertHelper.WarningDays &&
                !logged.Any(l => l.QueryId == q.Id && l.Action == "DelayAlert"))
            {
                db.AuditLogs.Add(new AuditLog
                {
                    QueryId = q.Id,
                    UserId = q.RaisedById,
                    Action = "DelayAlert",
                    Details = $"Query {q.QueryNumber} (IPO {q.IPO}) delayed {days} days - over the " +
                              $"{DelayAlertHelper.WarningDays}-day alert threshold. Manager alert raised.",
                    Timestamp = DateTime.UtcNow
                });
            }

            if (days > DelayAlertHelper.CriticalDays &&
                !logged.Any(l => l.QueryId == q.Id && l.Action == "AutoEmailSent"))
            {
                var subject = BuildSubject(q, days);
                var body = BuildBody(q, days);
                var to = managerEmails.Count > 0 ? string.Join(", ", managerEmails) : "office@iform.in";

                db.AuditLogs.Add(new AuditLog
                {
                    QueryId = q.Id,
                    UserId = q.RaisedById,
                    Action = "AutoEmailSent",
                    Details = $"Delay exceeded {DelayAlertHelper.CriticalDays} days ({days} days). " +
                              $"Auto email generated to managers.\nTo: {to}\nSubject: {subject}\n\n{body}",
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInformation(
                    "Auto email generated for query {QueryNumber} (IPO {Ipo}) delayed {Days} days.",
                    q.QueryNumber, q.IPO, days);
            }
        }

        await db.SaveChangesAsync(ct);
    }

    private static async Task<List<string>> GetManagerEmailsAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var managerRoleIds = await (from ur in db.UserRoles
                                    join r in db.Roles on ur.RoleId equals r.Id
                                    where r.Name == "Manager"
                                    select ur.UserId).ToListAsync(ct);

        return await db.Users
            .Where(u => managerRoleIds.Contains(u.Id) && u.IsActive && u.Email != null)
            .Select(u => u.Email!)
            .ToListAsync(ct);
    }

    private static string BuildSubject(SiteQuery q, int days)
        => $"URGENT - Site Query {q.QueryNumber} | IPO {q.IPO} delayed {days} days";

    private static string BuildBody(SiteQuery q, int days)
    {
        var project = q.Project?.Name ?? "-";
        var raisedBy = q.RaisedBy?.FullName ?? "-";
        return
            $"Dear Manager,\r\n\r\nThis is an automated delay alert. The following site query has exceeded the " +
            $"{DelayAlertHelper.CriticalDays}-day resolution window and requires your attention.\r\n\r\n" +
            $"Query Number: {q.QueryNumber}\r\nIPO: {q.IPO}\r\nProject: {project}\r\n" +
            $"Issue Type: {q.IssueType}\r\nRaised By: {raisedBy}\r\n" +
            $"Raised Date: {q.RaisedAt.ToLocalTime():dd/MM/yyyy}\r\nDelay: {days} days\r\nStatus: {q.Status}\r\n\r\n" +
            $"Please review and take corrective action at the earliest.\r\n\r\nThanks & Regards,\r\n" +
            "I-FORM Quality Portal - Automated Alert";
    }
}