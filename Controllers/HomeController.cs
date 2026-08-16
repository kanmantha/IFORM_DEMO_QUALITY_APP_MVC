using IFormQualityApp.Data;
using IFormQualityApp.Models;
using IFormQualityApp.Models.Entities;
using IFormQualityApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IFormQualityApp.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.UtcNow.Date;
        var queries = await _db.SiteQueries
            .Include(q => q.Project)
            .Include(q => q.RaisedBy)
            .ToListAsync();

        var openDays = queries
            .Where(q => q.Status != QueryStatus.Resolved)
            .Select(q => (now - q.RaisedAt.Date).Days)
            .ToList();

        var vm = new DashboardViewModel
        {
            TotalQueries = queries.Count,
            OpenQueries = queries.Count(q => q.Status == QueryStatus.Pending),
            InProgressQueries = queries.Count(q => q.Status == QueryStatus.InProgress),
            ResolvedQueries = queries.Count(q => q.Status == QueryStatus.Resolved),
            ActiveProjects = await _db.Projects.CountAsync(p => p.IsActive),
            ProductCount = await _db.Products.CountAsync(p => p.IsActive),
            MaxOpenDays = openDays.DefaultIfEmpty(0).Max(),
            AvgOpenDays = openDays.Count > 0 ? (int)Math.Round(openDays.Average()) : 0
        };

        vm.OpenDelays = queries
            .Where(q => q.Status != QueryStatus.Resolved)
            .OrderByDescending(q => (now - q.RaisedAt.Date).Days)
            .ThenBy(q => q.IPO)
            .Select(q => new QueryRowViewModel
            {
                Id = q.Id,
                QueryNumber = q.QueryNumber,
                IPO = q.IPO,
                Project = q.Project?.Name ?? "-",
                IssueType = q.IssueType.ToString(),
                Status = q.Status.ToString(),
                RaisedBy = q.RaisedBy?.FullName ?? "-",
                RaisedAt = q.RaisedAt,
                DelayDays = (now - q.RaisedAt.Date).Days,
                QtyNos = q.QtyNos,
                QtySqm = q.QtySqm
            })
            .ToList();

        vm.OpenByIssueType = new Dictionary<IssueType, int>
        {
            { IssueType.Missing, queries.Count(q => q.Status != QueryStatus.Resolved && q.IssueType == IssueType.Missing) },
            { IssueType.ProductionMistake, queries.Count(q => q.Status != QueryStatus.Resolved && q.IssueType == IssueType.ProductionMistake) },
            { IssueType.DesignMistake, queries.Count(q => q.Status != QueryStatus.Resolved && q.IssueType == IssueType.DesignMistake) },
            { IssueType.DispatchMissing, queries.Count(q => q.Status != QueryStatus.Resolved && q.IssueType == IssueType.DispatchMissing) }
        };

        ViewData["ActiveMenu"] = "Dashboard";
        return View(vm);
    }

    public IActionResult Home()
    {
        ViewData["ActiveMenu"] = "Home";
        return View();
    }

    public IActionResult UnderConstruction(string module)
    {
        ViewData["ActiveMenu"] = "Home";
        ViewData["ModuleName"] = module;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
