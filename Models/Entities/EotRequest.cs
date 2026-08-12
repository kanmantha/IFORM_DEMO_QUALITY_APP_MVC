namespace IFormQualityApp.Models.Entities;

public enum EotScenario
{
    SC1 = 1,
    SC2 = 2,
    SC3 = 3
}

public enum EotSubmissionStatus
{
    Draft = 0,
    Submitted = 1,
    InReview = 2,
    Approved = 3,
    Rejected = 4
}

public class EotRequest
{
    public int Id { get; set; }

    public string EotNumber { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public string Client { get; set; } = string.Empty;

    public string? FinancialYear { get; set; }

    public string? RevisionNo { get; set; }

    public EotScenario Scenario { get; set; }

    public DateTime? SpaDate { get; set; }

    public DateTime? DesignRevisionDate { get; set; }

    public string? ScopeVariation { get; set; }

    public int? DelayDays { get; set; }

    public string? CostEscalation { get; set; }

    public EotSubmissionStatus SubmissionStatus { get; set; } = EotSubmissionStatus.Draft;

    public string? ClientApproval { get; set; }

    public string? Remarks { get; set; }

    public string? ChangeProposedBy { get; set; }

    public string? Reason { get; set; }

    public string? Reference { get; set; }

    public string? OriginalScope { get; set; }

    public string? RevisedScope { get; set; }

    public string CreatedById { get; set; } = string.Empty;

    public AppUser? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
