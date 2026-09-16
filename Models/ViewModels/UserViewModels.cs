using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IFormQualityApp.Models.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Full name is required")]
    [MaxLength(100)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Employee code is required")]
    [MaxLength(50)]
    [Display(Name = "Employee code")]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required")]
    [MaxLength(100)]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Role")]
    public string Role { get; set; } = "SiteEngineer";
}

public class UserListViewModel
{
    public AppUserViewModel User { get; set; } = new();

    public string? CurrentRole { get; set; }
}

public class AppUserViewModel
{
    public string Id { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? EmployeeCode { get; set; }

    public string? Department { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}
