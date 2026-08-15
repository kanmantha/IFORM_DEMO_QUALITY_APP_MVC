using IFormQualityApp.Models.Entities;

namespace IFormQualityApp.Services;

public class EmailTemplateResult
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public static class EmailTemplateService
{
    /// <summary>
    /// Renders a stored email template (FR-5.2: wording auto-selects by issue type),
    /// replacing placeholders with query + sender data (FR-5.1: auto-fills from
    /// IPO, project, issue type and sender).
    /// </summary>
    public static EmailTemplateResult Render(SiteQuery query, AppUser sender, EmailTemplate? template = null)
    {
        template ??= DefaultFor(query.IssueType);

        var subject = ReplaceTokens(template.Subject, query, sender);
        var body = ReplaceTokens(template.Body, query, sender);
        return new EmailTemplateResult { Subject = subject, Body = body };
    }

    public static EmailTemplate DefaultFor(IssueType type)
    {
        return new EmailTemplate
        {
            Name = type.ToString(),
            IssueType = type,
            Subject = "Site Query - {IssueType} | IPO {IPO} | {Project}",
            Body =
                "Dear Team,\r\n\r\nThis is to notify that the following item has been identified on site.\r\n\r\n" +
                "IPO Number: {IPO}\r\nProject: {Project}\r\nIssue Type: {IssueType}\r\n" +
                "Quantity: {QtyNos} nos / {QtySqm} sqm\r\nRaised By: {Sender}\r\n" +
                "Date Raised: {RaisedAt}\r\nStatus: {Status}\r\n\r\n" +
                "Kindly take the necessary action at the earliest.\r\n\r\nThanks & Regards,\r\n" +
                "{Sender}\r\nI-FORM Aluminium & Design LLP"
        };
    }

    private static string ReplaceTokens(string input, SiteQuery query, AppUser sender)
    {
        var replacements = new Dictionary<string, string>
        {
            { "{IPO}", query.IPO },
            { "{Project}", query.Project?.Name ?? string.Empty },
            { "{IssueType}", query.IssueType.ToString() },
            { "{QtyNos}", query.QtyNos.ToString("0.##") },
            { "{QtySqm}", query.QtySqm.ToString("0.##") },
            { "{Sender}", sender.FullName },
            { "{RaisedAt}", query.RaisedAt.ToLocalTime().ToString("dd/MM/yyyy") },
            { "{Status}", query.Status.ToString() }
        };

        foreach (var kv in replacements)
        {
            input = input.Replace(kv.Key, kv.Value);
        }

        input = input.Replace("{br}", "\r\n");

        return input;
    }
}
