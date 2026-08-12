namespace IFormQualityApp.Models.Entities;

public class QueryComment
{
    public int Id { get; set; }

    public int QueryId { get; set; }

    public SiteQuery? Query { get; set; }

    public string UserId { get; set; } = string.Empty;

    public AppUser? User { get; set; }

    public string Text { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class AuditLog
{
    public int Id { get; set; }

    public int? QueryId { get; set; }

    public SiteQuery? Query { get; set; }

    public string Action { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public AppUser? User { get; set; }

    public string? Details { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
