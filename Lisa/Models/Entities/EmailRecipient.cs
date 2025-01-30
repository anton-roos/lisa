namespace Lisa.Models.Entities;

public class EmailRecipient
{
    public Guid? Id { get; set; }
    public Guid? EmailCampaignId { get; set; }
    public EmailCampaign? EmailCampaign { get; set; }
    public string? EmailAddress { get; set; }
    public EmailRecipientStatus Status { get; set; }
    public DateTime? OpenedAt { get; set; }
    public DateTime? ClickedAt { get; set; }
    public DateTime? BouncedAt { get; set; }
    public DateTime? UnsubscribedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
