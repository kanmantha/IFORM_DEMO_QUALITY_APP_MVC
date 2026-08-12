using IFormQualityApp.Models.Entities;

namespace IFormQualityApp.Services;

public class EmailTemplateResult
{
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
}

public static class EmailTemplateService
{
    public static EmailTemplateResult Render(SiteQuery query, AppUser sender)
    {
        var body = query.IssueType switch
        {
            IssueType.Missing =>
                "Dear Team,\r\n\r\nThis is to notify that the following item has been identified as MISSING on site.\r\n\r\n" +
                $"IPO Number: {query.IPO}\r\nProject: {query.Project?.Name}\r\nIssue Type: {query.IssueType}\r\n" +
                $"Quantity: {query.QtyNos} nos / {query.QtySqm} sqm\r\nRaised By: {sender.FullName}\r\n" +
                $"Date Raised: {query.RaisedAt:dd/MM/yyyy}\r\nStatus: {query.Status}\r\n\r\n" +
                "Kindly arrange for the dispatch of the missing item at the earliest.\r\n\r\nThanks & Regards,\r\n" +
                $"{sender.FullName}\r\nI-FORM Aluminium & Design LLP",
            IssueType.ProductionMistake =>
                "Dear Team,\r\n\r\nThis is to notify that the following item has been identified with a PRODUCTION MISTAKE on site.\r\n\r\n" +
                $"IPO Number: {query.IPO}\r\nProject: {query.Project?.Name}\r\nIssue Type: {query.IssueType}\r\n" +
                $"Quantity: {query.QtyNos} nos / {query.QtySqm} sqm\r\nRaised By: {sender.FullName}\r\n" +
                $"Date Raised: {query.RaisedAt:dd/MM/yyyy}\r\nStatus: {query.Status}\r\n\r\n" +
                "Please verify the production records and arrange for rectification or replacement.\r\n\r\nThanks & Regards,\r\n" +
                $"{sender.FullName}\r\nI-FORM Aluminium & Design LLP",
            IssueType.DesignMistake =>
                "Dear Team,\r\n\r\nThis is to notify that the following item has been identified with a DESIGN MISTAKE on site.\r\n\r\n" +
                $"IPO Number: {query.IPO}\r\nProject: {query.Project?.Name}\r\nIssue Type: {query.IssueType}\r\n" +
                $"Quantity: {query.QtyNos} nos / {query.QtySqm} sqm\r\nRaised By: {sender.FullName}\r\n" +
                $"Date Raised: {query.RaisedAt:dd/MM/yyyy}\r\nStatus: {query.Status}\r\n\r\n" +
                "Kindly review the drawings and advise the correct design/revision at the earliest.\r\n\r\nThanks & Regards,\r\n" +
                $"{sender.FullName}\r\nI-FORM Aluminium & Design LLP",
            _ =>
                "Dear Team,\r\n\r\nThis is to notify that the following item is MISSING FROM DISPATCH on site.\r\n\r\n" +
                $"IPO Number: {query.IPO}\r\nProject: {query.Project?.Name}\r\nIssue Type: {query.IssueType}\r\n" +
                $"Quantity: {query.QtyNos} nos / {query.QtySqm} sqm\r\nRaised By: {sender.FullName}\r\n" +
                $"Date Raised: {query.RaisedAt:dd/MM/yyyy}\r\nStatus: {query.Status}\r\n\r\n" +
                "Kindly verify the dispatch documents and arrange for the missing material.\r\n\r\nThanks & Regards,\r\n" +
                $"{sender.FullName}\r\nI-FORM Aluminium & Design LLP"
        };

        var subject = $"Site Query - {query.IssueType} | IPO {query.IPO} | {query.Project?.Name}";

        return new EmailTemplateResult { Subject = subject, Body = body };
    }
}
