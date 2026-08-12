using Microsoft.AspNetCore.Identity;

namespace IFormQualityApp.Models.Entities;

public class AppUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public string? EmployeeCode { get; set; }

    public string? Department { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
