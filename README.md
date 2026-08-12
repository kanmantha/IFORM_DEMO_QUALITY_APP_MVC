# I-FORM Quality Portal — Site Query & Defect Tracking (Web MVC + PostgreSQL)

ASP.NET Core MVC (.NET 10) web application implementing the **Site Query & Defect Tracking App**
described in the IFORM BRD / Project Proposal, plus the **EOT Management & Tracking Policy**
(IFAD-POL-EOT-001) and the **Aluminium Formwork Accessories product catalogue**.

Built with Entity Framework Core + Npgsql against PostgreSQL 16.

---

## Features

### Module 1 — Report
Raise a site query with issue type (Missing, Production Mistake, Design Mistake, Dispatch Missing),
quantity in **nos & sqm**, project, IPO number, optional photo evidence, optional linked product code,
and slab target/completed dates. Delay days are calculated automatically from the raise date.

### Module 2 — Search & Resolve
Search by IPO, project, or keyword; filter by issue type and status (AND logic).
- **Manager / Admin**: can move a query through Pending → In Progress → Resolved.
  Every resolve action is timestamped and tied to the resolving account (audit trail).
- **Site Engineer**: can view and comment on their own queries only; resolve is
  enforced at the **API level** (`[Authorize(Roles = "Manager,Admin")]`), not just hidden in the UI.

### Module 3 — Manager Dashboard
Live dashboard: total/open/in-progress/resolved counts, open delays ranked by severity
(descending delay days), breakdown by issue type with a doughnut chart, and max/avg open days.

### Module 4 — Product Code Lookup
Searchable catalogue of 118 aluminium formwork accessories (codes DAAA…DZAA0008) seeded from the
"Accessories with Photos" document. Filter by category or search by code/name/material.

### Module 5 — Auto-Generated Email Templates
Managers can generate an email template auto-filled from IPO, project, issue type, and sender,
with issue-type-specific wording. The email body/subject are editable before the action is logged.

### EOT Management
Full CRUD tracker per the EOT policy: EOT number auto-numbering, project, client, financial year,
revision, scenario (SC-1/2/3), SPA date, design revision date, scope variation (original/revised),
delay days, cost escalation, submission status, client approval, remarks, change proposer, and reference.

### Site Delivery Tracker
Excel-equivalent table view (Manager role) replicating every tracker column from the manual
spreadsheet, with print/PDF export.

---

## Roles & Demo Accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@iform.in` | `Iform@2026` |
| Manager | `venkatesh@iform.in`, `sowmya@iform.in`, `swapnika@iform.in` | `Iform@2026` |
| Site Engineer | `sai@iform.in`, `basha@iform.in`, `ramesh@iform.in`, `suresh@iform.in` | `Iform@2026` |

---

## Tech Stack

- **.NET 10** — ASP.NET Core MVC, Razor views
- **Entity Framework Core 10** + **Npgsql** (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **PostgreSQL 16** database
- **ASP.NET Core Identity** (cookie auth, roles)
- **Bootstrap 5.3** + Bootstrap Icons + Chart.js (professional dark-sidebar UI)

---

## Getting Started

### Prerequisites
- .NET SDK 10
- PostgreSQL 16 running on `localhost:5432`

### Configuration
Edit the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=iform_quality;Username=postgres;Password=postgres"
}
```

### Run

```bash
dotnet restore
dotnet ef database update   # applies migrations
dotnet run
```

On startup the app applies any pending migrations and seeds the database automatically
(roles, users, projects, product catalogue, sample queries, email templates), so
`dotnet run` alone is sufficient on a fresh database.

Open `http://localhost:5088` (or the URL printed by `dotnet run`).

---

## Project Structure

```
Controllers/      Account, Home (dashboard), Queries, Products, Eot, Tracker
Models/Entities/  AppUser, Project, SiteQuery, Product, EotRequest, EmailTemplate, comments/audit
Models/ViewModels Login, dashboard, query CRUD, catalogue, tracker view models
Data/             ApplicationDbContext (+ EF migrations)
Services/         DbSeeder, EmailTemplateService, DateHelpers
Views/            Razor views per controller + shared professional layout
wwwroot/          Bootstrap, Chart.js, custom CSS/JS, photo uploads
```

---

## Data Model Notes

- `SiteQuery` mirrors the manual tracker: IPO, project, issue type, qty (nos/sqm), dispatch status,
  slab target/completed/delay, raised-by, resolved-by, resolved-at, photo, verified product code.
- Delay days are always computed, never stored as a hand-entered field.
- `AuditLog` records every raise / status change / resolve / email / EOT action with user identity
  and timestamp (NFR-3).
- Role-based access control is enforced at the controller/action level (NFR-5).
