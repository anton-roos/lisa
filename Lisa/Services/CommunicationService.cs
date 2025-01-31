using Lisa.Models.Entities;

namespace Lisa.Services;

public class CommunicationService
(
    EmailCampaignService emailCampaignService,
    LearnerService learnerService,
    ILogger<CommunicationService> logger
)
{
    private readonly EmailCampaignService _emailCampaignService = emailCampaignService;
    private readonly LearnerService _learnerService = learnerService;
    private readonly ILogger<CommunicationService> _logger = logger;

    /// <summary>
    /// Sends an email campaign to all learners' parents in the selected school.
    /// </summary>
    public async Task<EmailCampaign?> SendForAllLearnersBySchool(School selectedSchool)
    {
        try
        {
            var learners = await _learnerService.GetLearnersBySchoolWithParentsAsync(selectedSchool.Id);
            if (learners.Count == 0)
            {
                _logger.LogWarning("No learners found for school ID {selectedSchool.Id}", selectedSchool.Id);
                return null;
            }

            var emailRecipients = new List<EmailRecipient>();
            var emailCampaignId = Guid.NewGuid();

            foreach (var learner in learners.Where(learner => learner.Parents != null))
            {
                var parents = learner.Parents!.ToList();

                foreach (var parent in parents)
                {
                    if (!string.IsNullOrWhiteSpace(parent.PrimaryEmail))
                    {
                        emailRecipients.Add(new EmailRecipient
                        {
                            Id = Guid.NewGuid(),
                            ParentId = parent.Id,
                            EmailAddress = parent.PrimaryEmail,
                            Status = EmailRecipientStatus.Pending,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            EmailCampaignId = emailCampaignId
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(parent.SecondaryEmail))
                    {
                        emailRecipients.Add(new EmailRecipient
                        {
                            Id = Guid.NewGuid(),
                            ParentId = parent.Id,
                            EmailAddress = parent.SecondaryEmail,
                            Status = EmailRecipientStatus.Pending,
                            UpdatedAt = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow,
                            EmailCampaignId = emailCampaignId
                        });
                    }
                }
            }

            if (emailRecipients.Count == 0)
            {
                _logger.LogWarning("No valid email recipients found for school ID {selectedSchool.Id}", selectedSchool.Id);
                return null;
            }

            var emailCampaign = new EmailCampaign
            {
                Id = emailCampaignId,
                Name = "School Notification",
                Description = "Automated notification to all parents",
                SubjectLine = "Important Update from Your School",
                SenderName = "School Admin",
                SenderEmail = "admin@school.com", // Replace with actual sender
                ContentHtml = "<p>This is an important message for parents.</p>",
                ContentText = "This is an important message for parents.",
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
            };

            var createdCampaign = await _emailCampaignService.CreateAsync(emailCampaign);
            _logger.LogInformation("Email campaign {createdCampaign.Id} created successfully.", createdCampaign.Id);

            return createdCampaign;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while sending email campaign for school {selectedSchool.Id}: {ex.Message}", selectedSchool.Id, ex.Message);
            return null;
        }
    }
}
