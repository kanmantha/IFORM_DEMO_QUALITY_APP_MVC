using IFormQualityApp.Data;
using IFormQualityApp.Models.Entities;
using IFormQualityApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IFormQualityApp.Controllers;

[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(
        ApplicationDbContext db,
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _db.Users.AsNoTracking().OrderBy(u => u.FullName).ToListAsync();
        var model = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            model.Add(new UserListViewModel
            {
                User = new AppUserViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? string.Empty,
                    EmployeeCode = user.EmployeeCode,
                    Department = user.Department,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                },
                CurrentRole = roles.FirstOrDefault() ?? "SiteEngineer"
            });
        }

        ViewData["Title"] = "User Management";
        ViewData["ActiveMenu"] = "Users";
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await RoleOptionsAsync();
        ViewData["Title"] = "Add User";
        ViewData["ActiveMenu"] = "Users";
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RegisterViewModel model, string role = "SiteEngineer")
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = await RoleOptionsAsync(role);
            ViewData["Title"] = "Add User";
            ViewData["ActiveMenu"] = "Users";
            return View(model);
        }

        var user = new AppUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            EmployeeCode = model.EmployeeCode,
            Department = model.Department,
            EmailConfirmed = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            await _userManager.AddToRoleAsync(user, role);
            TempData["Success"] = $"User {user.FullName} created with the {role} role.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        ViewBag.Roles = await RoleOptionsAsync(role);
        ViewData["Title"] = "Add User";
        ViewData["ActiveMenu"] = "Users";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(string id, string role)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "You cannot change your own role.";
            return RedirectToAction(nameof(Index));
        }

        if (!await _roleManager.RoleExistsAsync(role))
        {
            await _roleManager.CreateAsync(new IdentityRole(role));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        await _userManager.AddToRoleAsync(user, role);
        TempData["Success"] = $"{user.FullName} is now {role}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "You cannot deactivate your own account.";
            return RedirectToAction(nameof(Index));
        }

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);
        TempData["Success"] = $"{user.FullName} is now {(user.IsActive ? "active" : "deactivated")}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        var hasQuery = await _db.SiteQueries.AsNoTracking()
            .AnyAsync(q => q.RaisedById == user.Id || q.ResolvedById == user.Id);
        var hasComment = await _db.QueryComments.AsNoTracking()
            .AnyAsync(c => c.UserId == user.Id);
        var hasAudit = await _db.AuditLogs.AsNoTracking()
            .AnyAsync(a => a.UserId == user.Id);
        var hasEot = await _db.EotRequests.AsNoTracking()
            .AnyAsync(e => e.CreatedById == user.Id);

        if (hasQuery || hasComment || hasAudit || hasEot)
        {
            TempData["Error"] = $"{user.FullName} cannot be deleted because they have activity records. " +
                "Deactivate the account instead.";
            return RedirectToAction(nameof(Index));
        }

        var result = await _userManager.DeleteAsync(user);
        TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
            ? $"{user.FullName} deleted."
            : "Failed to delete user.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<string>> RoleOptionsAsync(string? selected = null)
    {
        var roles = await _roleManager.Roles.OrderBy(r => r.Name).Select(r => r.Name!).ToListAsync();
        if (roles.Count == 0)
        {
            roles = new List<string> { "Admin", "Manager", "SiteEngineer" };
        }

        if (selected is not null && !roles.Contains(selected))
        {
            roles.Add(selected);
        }

        return roles;
    }
}
