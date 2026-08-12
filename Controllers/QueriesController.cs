using IFormQualityApp.Data;
using IFormQualityApp.Models.Entities;
using IFormQualityApp.Models.ViewModels;
using IFormQualityApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IFormQualityApp.Controllers;

[Authorize]
public class QueriesController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AppUser> _userManager;

    public QueriesController(ApplicationDbContext db, UserManager<AppUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, IssueType? issueType, QueryStatus? status)
    {
        var isManager = User.IsInRole("Manager") || User.IsInRole("Admin");
        var user = await _userManager.GetUserAsync(User);

        IQueryable<SiteQuery> query = _db.SiteQueries
            .Include(q => q.Project)
            .Include(q => q.RaisedBy)
            .Include(q => q.ResolvedBy)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(q =>
                q.IPO.Contains(term) ||
                (q.Project != null && q.Project.Name.Contains(term)) ||
                q.QueryNumber.Contains(term) ||
                (q.Description != null && q.Description.Contains(term)) ||
                (q.RaisedBy != null && q.RaisedBy.FullName.Contains(term)));
        }

        if (issueType.HasValue)
        {
            query = query.Where(q => q.IssueType == issueType.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(q => q.Status == status.Value);
        }

        // Site engineers see their own queries only
        if (!isManager && user != null)
        {
            query = query.Where(q => q.RaisedById == user.Id);
        }

        var list = await query
            .OrderByDescending(q => q.RaisedAt)
            .Select(q => new QueryRowViewModel
            {
                Id = q.Id,
                QueryNumber = q.QueryNumber,
                IPO = q.IPO,
                Project = q.Project != null ? q.Project.Name : "-",
                IssueType = q.IssueType.ToString(),
                Status = q.Status.ToString(),
                RaisedBy = q.RaisedBy != null ? q.RaisedBy.FullName : "-",
                RaisedAt = q.RaisedAt,
                DelayDays = q.Status == QueryStatus.Resolved
                    ? ((q.ResolvedAt ?? q.RaisedAt).Date - q.RaisedAt.Date).Days
                    : (DateTime.UtcNow.Date - q.RaisedAt.Date).Days,
                QtyNos = q.QtyNos,
                QtySqm = q.QtySqm
            })
            .ToListAsync();

        var vm = new QueryListViewModel
        {
            Search = search,
            IssueTypeFilter = issueType,
            StatusFilter = status,
            IsManager = isManager,
            Queries = list
        };

        ViewData["ActiveMenu"] = "Queries";
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new CreateQueryViewModel();
        await PopulateLists(vm);
        ViewData["ActiveMenu"] = "Report";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateQueryViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLists(vm);
            return View(vm);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        string? photoPath = null;
        if (vm.Photo != null && vm.Photo.Length > 0)
        {
            var extension = Path.GetExtension(vm.Photo.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowed.Contains(extension) || vm.Photo.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(vm.Photo), "Please upload a valid image (jpg, png, webp, gif) up to 5 MB.");
                await PopulateLists(vm);
                return View(vm);
            }

            var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploads);
            var fileName = $"q_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploads, fileName);
            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await vm.Photo.CopyToAsync(stream);
            }

            photoPath = $"/uploads/{fileName}";
        }

        var query = new SiteQuery
        {
            IPO = vm.IPO.Trim(),
            ProjectId = vm.ProjectId,
            IssueType = vm.IssueType,
            QtyNos = vm.QtyNos,
            QtySqm = vm.QtySqm,
            Description = vm.Description,
            PhotoPath = photoPath,
            ProductCodeId = vm.ProductCodeId,
            SlabTargetDate = DateHelpers.ToUtc(vm.SlabTargetDate),
            SlabCompletedDate = DateHelpers.ToUtc(vm.SlabCompletedDate),
            RaisedById = user.Id,
            RaisedAt = DateTime.UtcNow,
            Status = QueryStatus.Pending,
            QueryNumber = await GenerateQueryNumberAsync()
        };

        if (vm.SlabTargetDate.HasValue && vm.SlabCompletedDate.HasValue)
        {
            query.SlabDelayDays = Math.Max(0, (vm.SlabCompletedDate.Value.Date - vm.SlabTargetDate.Value.Date).Days);
        }

        _db.SiteQueries.Add(query);
        await _db.SaveChangesAsync();

        _db.AuditLogs.Add(new AuditLog
        {
            QueryId = query.Id,
            UserId = user.Id,
            Action = "Raised",
            Details = $"Query {query.QueryNumber} raised with status Pending",
            Timestamp = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Query {query.QueryNumber} raised successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var isManager = User.IsInRole("Manager") || User.IsInRole("Admin");
        var user = await _userManager.GetUserAsync(User);

        var query = await _db.SiteQueries
            .Include(q => q.Project)
            .Include(q => q.RaisedBy)
            .Include(q => q.ResolvedBy)
            .Include(q => q.ProductCode)
            .Include(q => q.Comments).ThenInclude(c => c.User)
            .Include(q => q.AuditLogs.OrderByDescending(a => a.Timestamp)).ThenInclude(a => a.User)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (query == null)
        {
            return NotFound();
        }

        // Site engineers can only view their own queries
        if (!isManager && user != null && query.RaisedById != user.Id)
        {
            return Forbid();
        }

        var vm = new QueryDetailViewModel
        {
            Query = query,
            IsManager = isManager,
            CurrentUserId = user?.Id ?? string.Empty
        };

        ViewData["ActiveMenu"] = "Queries";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> ChangeStatus(int id, QueryStatus status)
    {
        var query = await _db.SiteQueries.FirstOrDefaultAsync(q => q.Id == id);
        if (query == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        var oldStatus = query.Status;

        if (status == QueryStatus.Resolved)
        {
            query.Status = QueryStatus.Resolved;
            query.ResolvedById = user?.Id;
            query.ResolvedAt = DateTime.UtcNow;
        }
        else if (status == QueryStatus.InProgress && query.Status == QueryStatus.Pending)
        {
            query.Status = QueryStatus.InProgress;
        }
        else if (status == QueryStatus.Pending)
        {
            query.Status = QueryStatus.Pending;
            query.ResolvedById = null;
            query.ResolvedAt = null;
        }

        query.UpdatedAt = DateTime.UtcNow;

        if (query.SlabTargetDate.HasValue && query.SlabCompletedDate.HasValue)
        {
            query.SlabDelayDays = Math.Max(0, (query.SlabCompletedDate.Value.Date - query.SlabTargetDate.Value.Date).Days);
        }

        _db.AuditLogs.Add(new AuditLog
        {
            QueryId = query.Id,
            UserId = user?.Id ?? string.Empty,
            Action = status == QueryStatus.Resolved ? "Resolved" : "StatusChanged",
            Details = $"Status changed from {oldStatus} to {query.Status}",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        TempData["Success"] = $"Query status updated to {query.Status}.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int id, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            TempData["Error"] = "Comment text cannot be empty.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var query = await _db.SiteQueries.FirstOrDefaultAsync(q => q.Id == id);
        if (query == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        var isManager = User.IsInRole("Manager") || User.IsInRole("Admin");

        if (!isManager && user != null && query.RaisedById != user.Id)
        {
            return Forbid();
        }

        _db.QueryComments.Add(new QueryComment
        {
            QueryId = query.Id,
            UserId = user?.Id ?? string.Empty,
            Text = text.Trim(),
            CreatedAt = DateTime.UtcNow
        });

        _db.AuditLogs.Add(new AuditLog
        {
            QueryId = query.Id,
            UserId = user?.Id ?? string.Empty,
            Action = "CommentAdded",
            Details = "Comment added to query",
            Timestamp = DateTime.UtcNow
        });

        query.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Comment added.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> Email(int id)
    {
        var query = await _db.SiteQueries
            .Include(q => q.Project)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (query == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        var rendered = EmailTemplateService.Render(query, user!);

        var vm = new QueryEmailViewModel
        {
            QueryId = query.Id,
            IPO = query.IPO,
            Project = query.Project?.Name ?? "-",
            IssueType = query.IssueType.ToString(),
            Sender = user?.FullName ?? string.Empty,
            Subject = rendered.Subject,
            Body = rendered.Body
        };

        ViewData["ActiveMenu"] = "Queries";
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> Email(QueryEmailViewModel vm)
    {
        var query = await _db.SiteQueries.FirstOrDefaultAsync(q => q.Id == vm.QueryId);
        if (query == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);

        _db.AuditLogs.Add(new AuditLog
        {
            QueryId = query.Id,
            UserId = user?.Id ?? string.Empty,
            Action = "EmailGenerated",
            Details = $"Auto email template generated - Subject: {vm.Subject}",
            Timestamp = DateTime.UtcNow
        });

        query.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Email template generated. A copy has been logged in the audit trail.";
        return RedirectToAction(nameof(Details), new { id = query.Id });
    }

    [HttpGet]
    public async Task<IActionResult> MyQueries()
    {
        return RedirectToAction(nameof(Index));
    }

    private async Task<string> GenerateQueryNumberAsync()
    {
        var prefix = $"Q-{DateTime.UtcNow:yyyyMMdd}";
        var count = await _db.SiteQueries.CountAsync(q => q.QueryNumber.StartsWith(prefix));
        return $"{prefix}-{count + 1:D3}";
    }

    private async Task PopulateLists(CreateQueryViewModel vm)
    {
        vm.Projects = await _db.Projects
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = p.Name })
            .ToListAsync();

        vm.ProductCodes = await _db.Products
            .Where(p => p.IsActive)
            .OrderBy(p => p.Code)
            .Select(p => new SelectListItem { Value = p.Id.ToString(), Text = $"{p.Code} - {p.Name}" })
            .ToListAsync();

        if (vm.ProductCodes.Count == 0)
        {
            vm.ProductCodes.Add(new SelectListItem { Value = "", Text = "No product codes available" });
        }
    }
}
