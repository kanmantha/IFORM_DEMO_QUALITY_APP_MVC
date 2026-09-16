using IFormQualityApp.Data;
using IFormQualityApp.Models.Entities;
using IFormQualityApp.Models.ViewModels;
using IFormQualityApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IFormQualityApp.Controllers;

[Authorize(Roles = "Manager,Admin")]
public class TrackerController : Controller
{
    private readonly ApplicationDbContext _db;

    public TrackerController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.UtcNow.Date;
        var rows = await _db.SiteQueries
            .Include(q => q.Project)
            .Include(q => q.RaisedBy)
            .OrderBy(q => q.RaisedAt)
            .Select(q => new TrackerRowViewModel
            {
                Id = q.Id,
                QueryNumber = q.QueryNumber,
                IPO = q.IPO,
                Project = q.Project != null ? q.Project.Name : "-",
                Issue = q.IssueType.ToString(),
                DispatchStatus = q.Status.ToString(),
                DelayDays = q.Status == QueryStatus.Resolved
                    ? ((q.ResolvedAt ?? q.RaisedAt).Date - q.RaisedAt.Date).Days
                    : (now - q.RaisedAt.Date).Days,
                DelayOver7 = q.Status != QueryStatus.Resolved &&
                             (now - q.RaisedAt.Date).Days > DelayAlertHelper.WarningDays,
                DelayOver30 = q.Status != QueryStatus.Resolved &&
                              (now - q.RaisedAt.Date).Days > DelayAlertHelper.CriticalDays,
                SlabTargetDate = q.SlabTargetDate,
                SlabCompletedDate = q.SlabCompletedDate,
                SlabDelayDays = q.SlabDelayDays,
                QtyNos = q.QtyNos,
                QtySqm = q.QtySqm,
                RaisedBy = q.RaisedBy != null ? q.RaisedBy.FullName : "-",
                RaisedAt = q.RaisedAt
            })
            .ToListAsync();

        var vm = new TrackerViewModel
        {
            Rows = rows,
            DelayAlerts = new DelayAlertSummary
            {
                WarningCount = rows.Count(r => r.DelayOver7 && !r.DelayOver30),
                CriticalCount = rows.Count(r => r.DelayOver30)
            }
        };
        ViewData["ActiveMenu"] = "Tracker";
        return View(vm);
    }
}
