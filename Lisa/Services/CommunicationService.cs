using Lisa.Models.Entities;

namespace Lisa.Services;

public class CommunicationService
(
    EmailCampaignService emailCampaignService,
    LearnerService learnerService
)
{
    private readonly EmailCampaignService _emailCampaignService = emailCampaignService;
    private readonly LearnerService _learnerService = learnerService;

    public async Task<EmailCampaign> SendForAllLearnersBySchool(School SelectedSchool)
    {
        var learners = await _learnerService.GetLearnersBySchoolWithParentsAsync(SelectedSchool.Id);
        var emailRecipients = new List<EmailRecipient>();

        foreach (var learner in learners)
        {
            if (learner.Parents == null) continue;
            
            foreach (var parent in learner.Parents)
            {
                emailRecipients.Add(new EmailRecipient
                {
                    EmailAddress = parent.PrimaryEmail,
                    Status = EmailRecipientStatus.Pending
                });

                if (parent.SecondaryEmail != null)
                {
                    emailRecipients.Add(new EmailRecipient
                    {
                        EmailAddress = parent.SecondaryEmail,
                        Status = EmailRecipientStatus.Pending
                    });
                }
            }
        }

        var emailCampaign = await _emailCampaignService.CreateAsync(new EmailCampaign
        {
            Name = "Test",
            Description = "Test",
            SubjectLine = "Test",
            SenderName = "Test",
            SenderEmail = "Test",
            ContentHtml = "Test",
            ContentText = "Test",
            Status = EmailCampaignStatus.Draft,
            ScheduledAt = DateTime.UtcNow,
            TrackOpens = true,
            TrackClicks = true,
            StatsSentCount = 0,
            StatsOpenCount = 0,
            StatsClickCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EmailRecipients = emailRecipients
        });

        return emailCampaign;
    }
}
