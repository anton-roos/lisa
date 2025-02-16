using System.Collections.Concurrent;
using Hangfire;
using Lisa.Data;
using Lisa.Enums;
using Lisa.Models.EmailModels;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class EmailCampaignService(
    IDbContextFactory<LisaDbContext> contextFactory,
    IUiEventService uiEventService,
    ILogger<EmailCampaignService> logger,
    EmailService emailService,
    LearnerService learnerService,
    UserService userService,
    EmailRendererService emailRendererService,
    EmailTemplateService emailTemplateService
)
{
    private readonly IDbContextFactory<LisaDbContext> _contextFactory = contextFactory;
    private readonly IUiEventService _uiEventService = uiEventService;
    private readonly ILogger<EmailCampaignService> _logger = logger;
    private static readonly ConcurrentDictionary<Guid, CancellationTokenSource> CampaignTokens = new();
    private readonly EmailService _emailService = emailService;
    private readonly LearnerService _learnerService = learnerService;
    private readonly UserService _userService = userService;
    private readonly EmailRendererService _emailRendererService = emailRendererService;
    private readonly EmailTemplateService _emailTemplateService = emailTemplateService;

    public async Task<List<EmailCampaign>> GetBySchoolIdAsync(Guid schoolId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        return await context.EmailCampaigns
            .Where(c => c.SchoolId == schoolId)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    /// Starts the email campaign and processes emails asynchronously using Hangfire.
    /// </summary>
    [JobDisplayName("Email Campaign Processing")]
    public async Task StartCampaignAsync(Guid campaignId)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var campaign = await context.EmailCampaigns
                .Include(c => c.EmailRecipients)
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign == null || campaign.Status == EmailCampaignStatus.Sent)
            {
                _logger.LogWarning("Campaign {campaignId} not found or already sent.", campaignId);
                return;
            }

            campaign.Status = EmailCampaignStatus.Sending;
            await context.SaveChangesAsync();

            await _uiEventService.PublishAsync(UiEvents.EmailCampaignStarted, new { campaign.Id });

            var tokenSource = new CancellationTokenSource();
            if (!CampaignTokens.TryAdd(campaignId, tokenSource))
            {
                _logger.LogError("Failed to add campaign {campaignId} to token dictionary.", campaignId);
                return;
            }

            await ProcessEmailsAsync(campaignId, tokenSource.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error starting campaign {campaignId}: {ex.Message}", campaignId, ex.Message);
        }
    }

    private async Task ProcessEmailsAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        try
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            var campaign = await context.EmailCampaigns
                .Include(c => c.EmailRecipients)
                .FirstOrDefaultAsync(c => c.Id == campaignId);

            if (campaign == null) return;

            int total = campaign.EmailRecipients?.Count ?? 0;
            int sent = 0;

            foreach (var recipient in campaign.EmailRecipients!)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (!string.IsNullOrWhiteSpace(recipient.EmailAddress))
                    {
                        string subject = campaign.SubjectLine ?? "No Subject";
                        string body = campaign.ContentHtml ?? "No Content";

                        BackgroundJob.Enqueue(() =>
                            SendEmailWithRetryAsync(recipient.EmailAddress, subject, body, campaign.SchoolId));
                        await Task.Delay(2000, cancellationToken);

                        recipient.Status = EmailRecipientStatus.Sent;
                    }
                    else
                    {
                        recipient.Status = EmailRecipientStatus.Bounced;
                    }

                    sent++;
                    var progress = (int)((double)sent / total * 100);

                    await _uiEventService.PublishAsync(UiEvents.EmailCampaignProgressUpdated, new
                    {
                        campaign.Id,
                        Progress = progress,
                        Total = total,
                        Sent = sent
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error sending email to {recipient.EmailAddress}: {ex.Message}",
                        recipient.EmailAddress, ex.Message);
                    recipient.Status = EmailRecipientStatus.Bounced;
                }
            }

            campaign.Status = EmailCampaignStatus.Sent;
            await context.SaveChangesAsync(cancellationToken);

            await _uiEventService.PublishAsync(UiEvents.EmailCampaignCompleted, new { campaign.Id });
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Email campaign {campaignId} was cancelled.", campaignId);
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error in ProcessEmailsAsync for campaign {campaignId}: {ex.Message}",
                campaignId, ex.Message);
        }
        finally
        {
            CampaignTokens.TryRemove(campaignId, out _);
        }
    }

    /// <summary>
    /// Sends an email with automatic retries using Hangfire.
    /// </summary>
    [AutomaticRetry(Attempts = 3)]
    public async Task SendEmailWithRetryAsync(string recipientEmailAddress, string subject, string body, Guid schoolId)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(recipientEmailAddress))
            {
                await _emailService.SendEmailAsync(schoolId: schoolId, to: recipientEmailAddress, subject: subject,
                    body: body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to send email to {recipient.EmailAddress}: {ex.Message}", recipientEmailAddress,
                ex.Message);
            throw;
        }
    }

    public async Task PauseCampaignAsync(Guid campaignId)
    {
        if (!TryCancelCampaign(campaignId))
            return;

        using var context = await _contextFactory.CreateDbContextAsync();
        var campaign = await context.EmailCampaigns.FindAsync(campaignId);
        if (campaign != null)
        {
            campaign.Status = EmailCampaignStatus.Paused;
            await context.SaveChangesAsync();
        }

        await _uiEventService.PublishAsync(UiEvents.EmailCampaignPaused, new { Id = campaignId });
    }

    public async Task StopCampaignAsync(Guid campaignId)
    {
        if (!TryCancelCampaign(campaignId))
            return;

        using var context = await _contextFactory.CreateDbContextAsync();
        var campaign = await context.EmailCampaigns.FindAsync(campaignId);
        if (campaign != null)
        {
            campaign.Status = EmailCampaignStatus.Cancelled;
            await context.SaveChangesAsync();
        }

        await _uiEventService.PublishAsync(UiEvents.EmailCampaignCancelled, new { Id = campaignId });
    }

    private bool TryCancelCampaign(Guid campaignId)
    {
        if (CampaignTokens.TryGetValue(campaignId, out var tokenSource))
        {
            tokenSource.Cancel();
            CampaignTokens.TryRemove(campaignId, out _);
            return true;
        }

        _logger.LogWarning("Attempted to cancel campaign {campaignId}, but no active task was found.", campaignId);
        return false;
    }

    public async Task<EmailCampaign?> GetByIdAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        return await context.EmailCampaigns.FindAsync(id);
    }

    /// <summary>
    /// Create a new EmailCampaign.
    /// </summary>
    public async Task<EmailCampaign> CreateAsync(CommunicationRequest request)
    {
        if (request != null)
        {
            var recipientEmails = await GetRecipientEmailsAsync(request);
            var template = await _emailTemplateService.GetTemplateByIdAsync(request.TemplateId);
            request.EmailTemplate = template;
            var html = await GenerateCampaignHtml(request);

            if (recipientEmails == null || recipientEmails.Count == 0)
                throw new ArgumentException("Recipient emails list cannot be empty.", nameof(recipientEmails));

            var utcNow = DateTime.UtcNow;

            // Determine a default subject based on the communication target if none is provided.
            var subjectLine = string.IsNullOrWhiteSpace(request.SubjectLine)
                ? request.Target switch
                {
                    CommunicationTarget.Learner => "Personalized Update for Our Learner",
                    CommunicationTarget.School => "Important Announcement to the School Community",
                    CommunicationTarget.SchoolGrade => "Grade-Specific Update",
                    CommunicationTarget.Subject => "Subject Update from Your School",
                    _ => "Important Update from Your School"
                }
                : request.SubjectLine;

            var emailCampaign = new EmailCampaign
            {
                Id = Guid.NewGuid(),
                Name = GenerateCampaignName(request),
                Description = $"Automated notification for {request.Audience} via {request.Target}",
                SubjectLine = subjectLine,
                SenderName = string.IsNullOrWhiteSpace(request.SenderName) ? "School Admin" : request.SenderName,
                SenderEmail = string.IsNullOrWhiteSpace(request.SenderEmail) ? "admin@school.com" : request.SenderEmail, // Consider sourcing this from configuration
                ContentHtml = html,
                Status = EmailCampaignStatus.Draft,
                ScheduledAt = utcNow.AddMinutes(1),
                TrackOpens = true,
                TrackClicks = true,
                StatsSentCount = 0,
                StatsOpenCount = 0,
                StatsClickCount = 0,
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
                EmailRecipients = [.. recipientEmails.Select(email => new EmailRecipient
                {
                    Id = Guid.NewGuid(),
                    EmailAddress = email,
                    Status = EmailRecipientStatus.Pending,
                    CreatedAt = utcNow,
                    UpdatedAt = utcNow
                })],
                SchoolId = request.SchoolId
            };

            using var context = await _contextFactory.CreateDbContextAsync();

            context.EmailCampaigns.Add(emailCampaign);
            await context.SaveChangesAsync();

            return emailCampaign;
        }

        throw new ArgumentNullException(nameof(request));
    }

    /// <summary>
    /// Gathers recipient emails based on the CommunicationRequest.
    /// </summary>
    private async Task<List<string>> GetRecipientEmailsAsync(CommunicationRequest request)
    {
        switch (request.Audience)
        {
            case Audience.Learners:
                return await GetLearnerEmailsAsync(request.SchoolId);

            case Audience.Staff:
                return await GetStaffEmailsAsync(request.SchoolId);

            case Audience.Parents:
                return await GetParentEmailsAsync(request.SchoolId);

            case Audience.LearnersAndStaff:
                var learners = await GetLearnerEmailsAsync(request.SchoolId);
                var staff = await GetStaffEmailsAsync(request.SchoolId);
                return learners.Concat(staff).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            case Audience.Grade:
                if (request.GradeId.HasValue)
                {
                    return await GetGradeEmailsAsync(request.GradeId.Value);
                }
                else
                {
                    _logger.LogWarning("Grade ID is null when retrieving grade emails.");
                    return [];
                }

            case Audience.Subject:
                return await GetSubjectEmailsAsync(request.SubjectId);

            default:
                _logger.LogWarning("Unknown audience type: {Audience}", request.Audience);
                return [];
        }
    }

    /// <summary>
    /// Generates a campaign name based on the audience and current timestamp.
    /// </summary>
    private static string GenerateCampaignName(CommunicationRequest request)
    {
        return $"{request.Target} - {request.Audience} Communication - {DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    /// <summary>
    /// Generates the HTML content for the campaign email based on the template type.
    /// If the template is of type "ProgressReportEmail", it renders a ProgressReportModel using the EmailRendererService.
    /// Otherwise, it falls back to using the provided ContentHtml.
    /// </summary>
    private async Task<string> GenerateCampaignHtml(CommunicationRequest request)
    {
        // Check if the request indicates that the email template is a ProgressReportEmail.
        // (Assuming request.TemplateModelType or similar is set to "ProgressReportEmail")
        if (request.EmailTemplate.Name?.Equals("Progress Report", StringComparison.OrdinalIgnoreCase) == true)
        {
            // Create a default ProgressReportModel. In a real scenario you might pull these values from the request or another data source.
            var progressReportModel = new ProgressReportModel
            {
                ParentName = "Default Parent",
                ChildName = "Default Child",
                Results = new List<Result>
            {
                new Result { Score = 95, Learner = new Learner { Surname = "Smith" } },
                new Result { Score = 87, Learner = new Learner { Surname = "Doe" } }
            }
            };

            var template = await _emailTemplateService.GetTemplateByIdAsync(request.TemplateId);

            // Use the EmailRendererService to render the HTML.
            // request.TemplateKey and request.TemplateContent should be provided in your CommunicationRequest.
            var renderedHtml = await _emailRendererService.RenderTemplateAsync(
                "template-" + template.Id,         // a unique key (could be the template ID)
                template.Content,     // the Razor template content
                progressReportModel);

            // Fallback in case rendering fails.
            return string.IsNullOrWhiteSpace(renderedHtml) ? "<p>No content available</p>" : renderedHtml;
        }
        else
        {
            // For other template types or if no template is provided, return the provided content or a default message.
            return "<p>No content available</p>";
        }
    }

    /// <summary>
    /// Retrieves parent emails for learners in a specific school.
    /// </summary>
    private async Task<List<string>> GetParentEmailsAsync(Guid? schoolId)
    {
        if (!schoolId.HasValue)
        {
            _logger.LogWarning("School ID is null when retrieving parent emails.");
            return [];
        }

        var learners = await _learnerService.GetLearnersBySchoolWithParentsAsync(schoolId.Value);
        var emails = learners
            .Where(l => l.Parents != null && l.Parents.Any())
            .SelectMany(l => l.Parents ?? Enumerable.Empty<Parent>())
            .SelectMany(p => new[] { p.PrimaryEmail, p.SecondaryEmail })
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return emails.Where(email => email != null).ToList()!;
    }

    /// <summary>
    /// Retrieves learner emails for a specific grade.
    /// </summary>
    private async Task<List<string>> GetGradeEmailsAsync(Guid gradeId)
    {
        var learners = await _learnerService.GetLearnersByGradeAsync(gradeId);
        var emails = learners
            .Select(l => l.Email)
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return emails.Where(email => email != null).ToList()!;
    }

    /// <summary>
    /// Retrieves learner emails for a specific subject.
    /// </summary>
    private async Task<List<string>> GetSubjectEmailsAsync(int subjectId)
    {
        var learners = await _learnerService.GetBySubjectIdAsync(subjectId);
        var emails = learners
            .Select(l => l.Email)
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return emails.Where(email => email != null).ToList()!;
    }

    /// <summary>
    /// Retrieves learner emails for a specific school.
    /// </summary>
    private async Task<List<string>> GetLearnerEmailsAsync(Guid? schoolId)
    {
        if (!schoolId.HasValue)
        {
            _logger.LogWarning("School ID is null when retrieving learner emails.");
            return new List<string>();
        }

        var learners = await _learnerService.GetLearnersBySchoolAsync(schoolId.Value);
        var emails = learners
            .Select(l => l.Email)
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return emails.Where(email => email != null).ToList()!;
    }

    /// <summary>
    /// Retrieves staff emails for a specific school.
    /// </summary>
    private async Task<List<string>> GetStaffEmailsAsync(Guid? schoolId)
    {
        if (!schoolId.HasValue)
        {
            _logger.LogWarning("School ID is null when retrieving staff emails.");
            return new List<string>();
        }

        var roles = new[] { Roles.Administrator, Roles.Principal, Roles.Teacher, Roles.SchoolManagement };
        var staff = await _userService.GetAllByRoleAndSchoolAsync(roles, schoolId.Value);
        var emails = staff
            .Select(s => s.Email)
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return emails.Where(email => email != null).ToList()!;
    }
}
