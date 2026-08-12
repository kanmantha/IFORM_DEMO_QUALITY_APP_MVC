namespace IFormQualityApp.Models.Entities;

public class Product
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Category { get; set; }

    public string? Description { get; set; }

    public string? Specification { get; set; }

    public string? Material { get; set; }

    public string? PhotoPath { get; set; }

    public int? ProjectId { get; set; }

    public Project? Project { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
