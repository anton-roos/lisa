namespace Lisa.Domain.Entities;

public enum EmailCampaignStatus
{
    Draft,
    Paused,
    Scheduled,
    Sending,
    Sent,
    Cancelled,
    Failed
}
