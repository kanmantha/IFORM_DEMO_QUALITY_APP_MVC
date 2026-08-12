namespace IFormQualityApp.Models.Entities;

public class Project
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Client { get; set; }

    public string? Location { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<SiteQuery> Queries { get; set; } = new List<SiteQuery>();

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
