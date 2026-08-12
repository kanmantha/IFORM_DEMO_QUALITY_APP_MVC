using IFormQualityApp.Models.Entities;

namespace IFormQualityApp.Models.ViewModels;

public class ProductListViewModel
{
    public string? Search { get; set; }

    public string? Category { get; set; }

    public List<string> Categories { get; set; } = new();

    public List<Product> Products { get; set; } = new();
}

public class EotListViewModel
{
    public string? Search { get; set; }

    public EotSubmissionStatus? StatusFilter { get; set; }

    public List<EotRequest> Eots { get; set; } = new();
}

public class EotEditViewModel
{
    public int? Id { get; set; }

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

    public EotSubmissionStatus SubmissionStatus { get; set; }

    public string? ClientApproval { get; set; }

    public string? Remarks { get; set; }

    public string? ChangeProposedBy { get; set; }

    public string? Reason { get; set; }

    public string? Reference { get; set; }

    public string? OriginalScope { get; set; }

    public string? RevisedScope { get; set; }
}
