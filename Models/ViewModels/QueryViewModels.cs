using IFormQualityApp.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IFormQualityApp.Models.ViewModels;

public class CreateQueryViewModel
{
    public string IPO { get; set; } = string.Empty;

    public int ProjectId { get; set; }

    public IssueType IssueType { get; set; }

    public decimal QtyNos { get; set; }

    public decimal QtySqm { get; set; }

    public string? Description { get; set; }

    public IFormFile? Photo { get; set; }

    public int? ProductCodeId { get; set; }

    public DateTime? SlabTargetDate { get; set; }

    public DateTime? SlabCompletedDate { get; set; }

    public List<SelectListItem> Projects { get; set; } = new();

    public List<SelectListItem> ProductCodes { get; set; } = new();
}

public class QueryListViewModel
{
    public string? Search { get; set; }

    public IssueType? IssueTypeFilter { get; set; }

    public QueryStatus? StatusFilter { get; set; }

    public bool IsManager { get; set; }

    public List<QueryRowViewModel> Queries { get; set; } = new();
}

public class QueryDetailViewModel
{
    public SiteQuery Query { get; set; } = new();

    public bool IsManager { get; set; }

    public string CurrentUserId { get; set; } = string.Empty;

    public string? NewComment { get; set; }
}

public class QueryEmailViewModel
{
    public int QueryId { get; set; }

    public int TemplateId { get; set; }

    public string IPO { get; set; } = string.Empty;

    public string Project { get; set; } = string.Empty;

    public string IssueType { get; set; } = string.Empty;

    public string Sender { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string To { get; set; } = string.Empty;

    public string Cc { get; set; } = string.Empty;

    public List<SelectListItem> Templates { get; set; } = new();
}
