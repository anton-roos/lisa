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
    IEnumerable<ICampaignTemplateProcessor> templateProcessors
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
    private readonly IEnumerable<ICampaignTemplateProcessor> _templateProcessors = templateProcessors;

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

            await _uiEventService.PublishAsync(UiEvents.EmailCampaignProgressUpdated, new
            {
                campaign.Id,
                Progress = 0,
                Total = total,
                Sent = sent
            });

            foreach (var recipient in campaign.EmailRecipients!)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    if (!string.IsNullOrWhiteSpace(recipient.EmailAddress))
                    {
                        string subject = campaign.SubjectLine ?? "No Subject";
                        string body = campaign.ContentHtml!;

                        if (campaign.EmailTemplate == Template.ProgressReport && recipient.LearnerId.HasValue)
                        {
                            body = await _emailRendererService.RenderProgressFeedbackAsync(recipient.LearnerId.Value);
                        }

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
                    _logger.LogError("Error sending email to {recipientEmail}: {error}",
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
            _logger.LogError("Unexpected error in ProcessEmailsAsync for campaign {campaignId}: {error}",
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
    public async Task<EmailCampaign> CreateAsync(CommunicationCommand command)
    {
        if (command is null) throw new ArgumentNullException(nameof(command));

        var processor = _templateProcessors.FirstOrDefault(p => p.CanProcess(command.EmailTemplate))
                        ?? throw new InvalidOperationException("No processor found for the given template.");

        var html = await processor.GenerateHtmlAsync(command);
        await processor.ProcessAdditionalActionsAsync(command);

        var utcNow = DateTime.UtcNow;
        var subjectLine = string.IsNullOrWhiteSpace(command.SubjectLine)
            ? command.Target switch
            {
                CommunicationTarget.Learner => "Personalized Update for Our Learner",
                CommunicationTarget.School => "Important Announcement to the School Community",
                CommunicationTarget.SchoolGrade => "Grade-Specific Update",
                CommunicationTarget.Subject => "Subject Update from Your School",
                _ => "Important Update from Your School"
            }
            : command.SubjectLine;

        List<EmailRecipient> recipients;

        if (command.EmailTemplate == Template.ProgressReport)
        {
            var progressRecipients = await GetProgressReportRecipientsAsync(command);
            if (!progressRecipients.Any())
                throw new ArgumentException("No recipients found for the progress report.", nameof(progressRecipients));

            recipients = progressRecipients.Select(r => new EmailRecipient
            {
                Id = Guid.NewGuid(),
                EmailAddress = r.Email,
                LearnerId = r.LearnerId,
                Status = EmailRecipientStatus.Pending,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            }).ToList();
        }
        else
        {
            var recipientEmails = await GetRecipientEmailsAsync(command);
            if (recipientEmails == null || !recipientEmails.Any())
                throw new ArgumentException("Recipient emails list cannot be empty.", nameof(recipientEmails));

            recipients = recipientEmails.Select(email => new EmailRecipient
            {
                Id = Guid.NewGuid(),
                EmailAddress = email,
                Status = EmailRecipientStatus.Pending,
                CreatedAt = utcNow,
                UpdatedAt = utcNow
            }).ToList();
        }

        var emailCampaign = new EmailCampaign
        {
            Id = Guid.NewGuid(),
            Name = GenerateCampaignName(command),
            Description = $"Automated notification for {command.Audience} via {command.Target}",
            SubjectLine = subjectLine,
            SenderName = string.IsNullOrWhiteSpace(command.SenderName) ? "School Admin" : command.SenderName,
            SenderEmail = string.IsNullOrWhiteSpace(command.SenderEmail) ? "admin@school.com" : command.SenderEmail,
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
            EmailRecipients = recipients,
            SchoolId = command.SchoolId,
            EmailTemplate = command.EmailTemplate
        };

        using var context = await _contextFactory.CreateDbContextAsync();
        context.EmailCampaigns.Add(emailCampaign);
        await context.SaveChangesAsync();

        return emailCampaign;
    }

    private async Task<List<(string Email, Guid LearnerId)>> GetProgressReportRecipientsAsync(CommunicationCommand command)
    {
        List<Learner> learners;

        if (command.Target == CommunicationTarget.Learner && command.LearnerId.HasValue)
        {
            var learner = await _learnerService.GetByIdAsync(command.LearnerId.Value);
            learners = learner != null ? [learner] : [];
        }
        else
        {
            switch (command.Audience)
            {
                case Audience.Parents:
                case Audience.LearnersAndStaff:
                    learners = await _learnerService.GetLearnersBySchoolAsync(command.SchoolId);
                    break;
                case Audience.Grade:
                    if (command.GradeId.HasValue)
                        learners = await _learnerService.GetLearnersByGradeAsync(command.GradeId.Value);
                    else
                        learners = new List<Learner>();
                    break;
                case Audience.Subject:
                    learners = await _learnerService.GetBySubjectIdAsync(command.SubjectId);
                    break;
                default:
                    learners = await _learnerService.GetLearnersBySchoolAsync(command.SchoolId);
                    break;
            }
        }

        var recipients = new List<(string Email, Guid LearnerId)>();

        foreach (var learner in learners)
        {
            if (learner.Parents != null)
            {
                foreach (var parent in learner.Parents)
                {
                    string? email = !string.IsNullOrWhiteSpace(parent.PrimaryEmail)
                        ? parent.PrimaryEmail
                        : parent.SecondaryEmail;
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        recipients.Add((email, learner.Id));
                    }
                }
            }
        }

        return recipients;
    }


    /// <summary>
    /// Gathers recipient emails based on the CommunicationRequest.
    /// </summary>
    private async Task<List<string>> GetRecipientEmailsAsync(CommunicationCommand command)
    {
        switch (command.Audience)
        {
            case Audience.Learners:
                return await GetLearnerEmailsAsync(command.SchoolId);

            case Audience.Staff:
                return await GetStaffEmailsAsync(command.SchoolId);

            case Audience.Parents:
                return await GetParentEmailsAsync(command.SchoolId);

            case Audience.LearnersAndStaff:
                var learners = await GetLearnerEmailsAsync(command.SchoolId);
                var staff = await GetStaffEmailsAsync(command.SchoolId);
                return learners.Concat(staff).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            case Audience.Grade:
                if (command.GradeId.HasValue)
                {
                    return await GetGradeEmailsAsync(command.GradeId.Value);
                }
                else
                {
                    _logger.LogWarning("Grade ID is null when retrieving grade emails.");
                    return new List<string>();
                }

            case Audience.Subject:
                return await GetSubjectEmailsAsync(command.SubjectId);

            default:
                _logger.LogWarning("Unknown audience type: {Audience}", command.Audience);
                return new List<string>();
        }
    }

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

        return emails;
    }

    /// <summary>
    /// Generates a campaign name based on the audience and current timestamp.
    /// </summary>
    private static string GenerateCampaignName(CommunicationCommand command)
    {
        return $"{command.Target} - {command.Audience} Communication - {DateTime.UtcNow:yyyyMMddHHmmss}";
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

        return emails;
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

    /// <summary>
    /// Sends a personalized progress report email to each parent of every learner.
    /// Each email is rendered with a ProgressReportModel specific to the learner.
    /// </summary>
    /// <param name="schoolId">The school identifier.</param>
    /// <param name="templateId">The template ID for the progress report email.</param>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task SendProgressReportsAsync(Guid schoolId)
    {
        var learners = await _learnerService.GetLearnersBySchoolAsync(schoolId);
        if (learners == null || learners.Count == 0)
        {
            _logger.LogWarning("No learners found for school {SchoolId}.", schoolId);
            return;
        }

        foreach (var learner in learners)
        {
            if (learner.Parents == null || learner.Parents.Count == 0)
            {
                _logger.LogWarning("Learner {LearnerId} has no associated parents.", learner.Id);
                continue;
            }

            // Render the personalized HTML content using the new Razor page renderer.
            var renderedHtml = await _emailRendererService.RenderProgressFeedbackAsync(learner.Id);
            var subject = "Progress Report";
            foreach (var parent in learner.Parents)
            {
                if (string.IsNullOrWhiteSpace(parent.PrimaryEmail) && string.IsNullOrWhiteSpace(parent.SecondaryEmail))
                {
                    _logger.LogWarning("Parent for learner {LearnerId} does not have a valid email.", learner.Id);
                    continue;
                }

                var recipientEmail = !string.IsNullOrWhiteSpace(parent.PrimaryEmail)
                    ? parent.PrimaryEmail
                    : parent.SecondaryEmail;

                BackgroundJob.Enqueue(() =>
                    SendEmailWithRetryAsync(recipientEmail, subject, renderedHtml, schoolId));

                _logger.LogInformation("Scheduled progress report email for learner {LearnerId} to parent {ParentEmail}.",
                    learner.Id, recipientEmail);
            }
        }
    }
}
