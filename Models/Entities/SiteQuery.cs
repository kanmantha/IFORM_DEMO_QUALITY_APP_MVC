namespace IFormQualityApp.Models.Entities;

public enum IssueType
{
    Missing = 0,
    ProductionMistake = 1,
    DesignMistake = 2,
    DispatchMissing = 3
}

public enum QueryStatus
{
    Pending = 0,
    InProgress = 1,
    Resolved = 2
}

public class SiteQuery
{
    public int Id { get; set; }

    public string QueryNumber { get; set; } = string.Empty;

    public string IPO { get; set; } = string.Empty;

    public int ProjectId { get; set; }

    public Project? Project { get; set; }

    public IssueType IssueType { get; set; }

    public QueryStatus Status { get; set; } = QueryStatus.Pending;

    public decimal QtyNos { get; set; }

    public decimal QtySqm { get; set; }

    public string? Description { get; set; }

    public string? PhotoPath { get; set; }

    public int? ProductCodeId { get; set; }

    public Product? ProductCode { get; set; }

    public DateTime? SlabTargetDate { get; set; }

    public DateTime? SlabCompletedDate { get; set; }

    public int? SlabDelayDays { get; set; }

    public string RaisedById { get; set; } = string.Empty;

    public AppUser? RaisedBy { get; set; }

    public DateTime RaisedAt { get; set; } = DateTime.UtcNow;

    public string? ResolvedById { get; set; }

    public AppUser? ResolvedBy { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<QueryComment> Comments { get; set; } = new List<QueryComment>();

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
