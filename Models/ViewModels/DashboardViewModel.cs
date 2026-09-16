using IFormQualityApp.Models.Entities;

namespace IFormQualityApp.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalQueries { get; set; }
    public int OpenQueries { get; set; }
    public int InProgressQueries { get; set; }
    public int ResolvedQueries { get; set; }
    public int ActiveProjects { get; set; }
    public int ProductCount { get; set; }
    public int AvgOpenDays { get; set; }
    public int MaxOpenDays { get; set; }

    public DelayAlertSummary DelayAlerts { get; set; } = new();

    public List<QueryRowViewModel> OpenDelays { get; set; } = new();

    public Dictionary<IssueType, int> OpenByIssueType { get; set; } = new();
}

public class DelayAlertSummary
{
    /// <summary>Open queries delayed over 7 days (not yet past 30 days).</summary>
    public int WarningCount { get; set; }

    /// <summary>Open queries delayed over 30 days (auto email triggered).</summary>
    public int CriticalCount { get; set; }

    public bool HasWarnings => WarningCount > 0;

    public bool HasCritical => CriticalCount > 0;
}

public class QueryRowViewModel
{
    public int Id { get; set; }
    public string QueryNumber { get; set; } = string.Empty;
    public string IPO { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string IssueType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RaisedBy { get; set; } = string.Empty;
    public DateTime RaisedAt { get; set; }
    public int DelayDays { get; set; }
    public bool DelayOver7 { get; set; }
    public bool DelayOver30 { get; set; }
    public decimal QtyNos { get; set; }
    public decimal QtySqm { get; set; }
}
