namespace IFormQualityApp.Models.ViewModels;

public class TrackerRowViewModel
{
    public int Id { get; set; }
    public string QueryNumber { get; set; } = string.Empty;
    public string IPO { get; set; } = string.Empty;
    public string Project { get; set; } = string.Empty;
    public string Issue { get; set; } = string.Empty;
    public string DispatchStatus { get; set; } = string.Empty;
    public int DelayDays { get; set; }
    public DateTime? SlabTargetDate { get; set; }
    public DateTime? SlabCompletedDate { get; set; }
    public int? SlabDelayDays { get; set; }
    public decimal QtyNos { get; set; }
    public decimal QtySqm { get; set; }
    public string RaisedBy { get; set; } = string.Empty;
    public DateTime RaisedAt { get; set; }
}

public class TrackerViewModel
{
    public List<TrackerRowViewModel> Rows { get; set; } = new();
}
