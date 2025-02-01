namespace Lisa.Models.Entities;

public class EmailCampaign
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? SubjectLine { get; set; }
    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }
    public string? ContentHtml { get; set; }
    public string? ContentText { get; set; }
    public EmailCampaignStatus Status { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool TrackOpens { get; set; } = true;
    public bool TrackClicks { get; set; } = true;
    public int StatsSentCount { get; set; }
    public int StatsOpenCount { get; set; }
    public int StatsClickCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<EmailRecipient>? EmailRecipients { get; set; }
    public Guid SchoolId { get; set; }
}
