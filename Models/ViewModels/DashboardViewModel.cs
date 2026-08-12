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

    public List<QueryRowViewModel> OpenDelays { get; set; } = new();

    public Dictionary<IssueType, int> OpenByIssueType { get; set; } = new();
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
    public decimal QtyNos { get; set; }
    public decimal QtySqm { get; set; }
}
