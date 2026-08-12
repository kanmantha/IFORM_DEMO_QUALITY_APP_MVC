namespace IFormQualityApp.Models.Entities;

public class EmailTemplate
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public IssueType IssueType { get; set; }

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
