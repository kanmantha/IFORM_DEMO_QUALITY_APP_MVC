using IFormQualityApp.Data;
using IFormQualityApp.Models.Entities;
using IFormQualityApp.Models.ViewModels;
using IFormQualityApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IFormQualityApp.Controllers;

[Authorize]
public class EotController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public EotController(ApplicationDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, EotSubmissionStatus? status)
    {
        IQueryable<EotRequest> query = _db.EotRequests
            .Include(e => e.CreatedBy)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(e =>
                e.EotNumber.Contains(term) ||
                e.ProjectName.Contains(term) ||
                e.Client.Contains(term));
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.SubmissionStatus == status.Value);
        }

        var list = await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        var vm = new EotListViewModel
        {
            Search = search,
            StatusFilter = status,
            Eots = list
        };

        ViewData["ActiveMenu"] = "EOT";
        return View(vm);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var vm = new EotEditViewModel
        {
            FinancialYear = DateTime.UtcNow.Year.ToString(),
            SubmissionStatus = EotSubmissionStatus.Draft
        };
        ViewData["ActiveMenu"] = "EOT";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EotEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var count = await _db.EotRequests.CountAsync() + 1;
        var eot = new EotRequest
        {
            EotNumber = $"EOT-{count:D2}",
            ProjectName = vm.ProjectName.Trim(),
            Client = vm.Client.Trim(),
            FinancialYear = vm.FinancialYear,
            RevisionNo = vm.RevisionNo,
            Scenario = vm.Scenario,
            SpaDate = DateHelpers.ToUtc(vm.SpaDate),
            DesignRevisionDate = DateHelpers.ToUtc(vm.DesignRevisionDate),
            ScopeVariation = vm.ScopeVariation,
            DelayDays = vm.DelayDays,
            CostEscalation = vm.CostEscalation,
            SubmissionStatus = vm.SubmissionStatus,
            ClientApproval = vm.ClientApproval,
            Remarks = vm.Remarks,
            ChangeProposedBy = vm.ChangeProposedBy,
            Reason = vm.Reason,
            Reference = vm.Reference,
            OriginalScope = vm.OriginalScope,
            RevisedScope = vm.RevisedScope,
            CreatedById = user.Id,
            CreatedAt = DateTime.UtcNow
        };

        _db.EotRequests.Add(eot);
        await _db.SaveChangesAsync();

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = user.Id,
            Action = "EOTCreated",
            Details = $"EOT {eot.EotNumber} created for project {eot.ProjectName}",
            Timestamp = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = $"EOT {eot.EotNumber} created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var eot = await _db.EotRequests.FirstOrDefaultAsync(e => e.Id == id);
        if (eot == null)
        {
            return NotFound();
        }

        var vm = new EotEditViewModel
        {
            Id = eot.Id,
            EotNumber = eot.EotNumber,
            ProjectName = eot.ProjectName,
            Client = eot.Client,
            FinancialYear = eot.FinancialYear,
            RevisionNo = eot.RevisionNo,
            Scenario = eot.Scenario,
            SpaDate = eot.SpaDate,
            DesignRevisionDate = eot.DesignRevisionDate,
            ScopeVariation = eot.ScopeVariation,
            DelayDays = eot.DelayDays,
            CostEscalation = eot.CostEscalation,
            SubmissionStatus = eot.SubmissionStatus,
            ClientApproval = eot.ClientApproval,
            Remarks = eot.Remarks,
            ChangeProposedBy = eot.ChangeProposedBy,
            Reason = eot.Reason,
            Reference = eot.Reference,
            OriginalScope = eot.OriginalScope,
            RevisedScope = eot.RevisedScope
        };

        ViewData["ActiveMenu"] = "EOT";
        return View("Create", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EotEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View("Create", vm);
        }

        var eot = await _db.EotRequests.FirstOrDefaultAsync(e => e.Id == vm.Id);
        if (eot == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        var oldStatus = eot.SubmissionStatus;

        eot.ProjectName = vm.ProjectName.Trim();
        eot.Client = vm.Client.Trim();
        eot.FinancialYear = vm.FinancialYear;
        eot.RevisionNo = vm.RevisionNo;
        eot.Scenario = vm.Scenario;
        eot.SpaDate = DateHelpers.ToUtc(vm.SpaDate);
        eot.DesignRevisionDate = DateHelpers.ToUtc(vm.DesignRevisionDate);
        eot.ScopeVariation = vm.ScopeVariation;
        eot.DelayDays = vm.DelayDays;
        eot.CostEscalation = vm.CostEscalation;
        eot.SubmissionStatus = vm.SubmissionStatus;
        eot.ClientApproval = vm.ClientApproval;
        eot.Remarks = vm.Remarks;
        eot.ChangeProposedBy = vm.ChangeProposedBy;
        eot.Reason = vm.Reason;
        eot.Reference = vm.Reference;
        eot.OriginalScope = vm.OriginalScope;
        eot.RevisedScope = vm.RevisedScope;
        eot.UpdatedAt = DateTime.UtcNow;

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = user?.Id ?? string.Empty,
            Action = "EOTUpdated",
            Details = $"EOT {eot.EotNumber} updated (status {oldStatus} -> {eot.SubmissionStatus})",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        TempData["Success"] = $"EOT {eot.EotNumber} updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var eot = await _db.EotRequests.FirstOrDefaultAsync(e => e.Id == id);
        if (eot == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);

        _db.AuditLogs.Add(new AuditLog
        {
            UserId = user?.Id ?? string.Empty,
            Action = "EOTDeleted",
            Details = $"EOT {eot.EotNumber} deleted for project {eot.ProjectName}",
            Timestamp = DateTime.UtcNow
        });

        _db.EotRequests.Remove(eot);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"EOT {eot.EotNumber} deleted.";
        return RedirectToAction(nameof(Index));
    }
}
