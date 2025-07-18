using Lisa.Enums;

namespace Lisa.Models.Entities;

public class EmailCampaign
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? SubjectLine { get; set; }
    public EmailCampaignStatus Status { get; set; }
    public bool TrackOpens { get; set; } = true;
    public bool TrackClicks { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<EmailRecipient>? EmailRecipients { get; set; }
    public Guid SchoolId { get; set; }
    public RecipientTemplate RecipientTemplate { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
