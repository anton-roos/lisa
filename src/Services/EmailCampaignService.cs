using Lisa.Data;
using Lisa.Enums;
using Lisa.Models.EmailModels;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Lisa.Services;

public class EmailCampaignService
(
    IDbContextFactory<LisaDbContext> contextFactory,
    UiEventService uiEventService,
    ILogger<EmailCampaignService> logger,
    EmailService emailService,
    LearnerService learnerService,
    UserService userService,
    TemplateRenderService templateRenderService,
    ProgressFeedbackService progressFeedbackService
)
{
    private static readonly ConcurrentDictionary<Guid, CancellationTokenSource> CampaignIds = new();
    private const int BatchSize = 100;
    private const int ProgressComplete = 100;
    private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public async Task<EmailCampaign> CreateAsync(CommunicationCommand command)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            var recipients = await GenerateRecipientsAsync(command);
            var emailCampaign = new EmailCampaign
            {
                Id = Guid.NewGuid(),
                Name = GenerateCampaignName(command),
                Status = EmailCampaignStatus.Draft,
                TrackOpens = true,
                TrackClicks = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                EmailRecipients = recipients,
                SchoolId = command.SchoolId,
                RecipientTemplate = command.RecipientTemplate,
                FromDate = command.FromDate.HasValue ? command.FromDate.Value : null,
                ToDate = command.ToDate.HasValue ? command.ToDate.Value : null,
            };

            context.EmailCampaigns.Add(emailCampaign);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            return emailCampaign;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            logger.LogError(ex, "Failed to create campaign for SchoolId {SchoolId}", command.SchoolId);
            throw;
        }
    }

    public async Task<List<EmailCampaign>> GetBySchoolIdAsync(Guid schoolId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.EmailCampaigns
            .Include(c => c.EmailRecipients)
            .Where(c => c.SchoolId == schoolId)
            .OrderByDescending(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<EmailCampaign?> GetByIdAsync(Guid id)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.EmailCampaigns
            .Include(c => c.EmailRecipients)
                .ThenInclude(r => r.Learner)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task StartCampaignAsync(Guid campaignId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var campaign = await context.EmailCampaigns
            .Include(c => c.EmailRecipients)
            .FirstOrDefaultAsync(c => c.Id == campaignId);

        if (campaign == null)
        {
            logger.LogWarning("Campaign {CampaignId} not found.", campaignId);
            return;
        }

        if (campaign.Status == EmailCampaignStatus.Sent)
        {
            logger.LogWarning("Campaign {CampaignId} already sent.", campaignId);
            return;
        }

        if (campaign.Status == EmailCampaignStatus.Cancelled)
        {
            logger.LogWarning("Campaign {CampaignId} is cancelled and cannot be restarted.", campaignId);
            return;
        }

        if (CampaignIds.ContainsKey(campaignId))
        {
            logger.LogWarning("Campaign {CampaignId} is already running.", campaignId);
            return;
        }

        var tokenSource = new CancellationTokenSource();
        if (!CampaignIds.TryAdd(campaignId, tokenSource))
        {
            logger.LogWarning("Campaign {CampaignId} is already running.", campaignId);
            tokenSource.Dispose();
            return;
        }

        try
        {
            if (!ValidateCampaign(campaign))
            {
                campaign.Status = EmailCampaignStatus.Failed;
                campaign.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
                return;
            }

            // If resuming a campaign, skip already processed recipients
            var pendingRecipients = campaign.EmailRecipients?
                .Where(r => r.Status == EmailRecipientStatus.Pending)
                .ToList() ?? [];

            if (pendingRecipients.Count == 0)
            {
                logger.LogInformation("Campaign {CampaignId} has no pending recipients - marking as complete", campaignId);
                campaign.Status = EmailCampaignStatus.Sent;
                campaign.UpdatedAt = DateTime.UtcNow;
                await context.SaveChangesAsync();
                await uiEventService.PublishAsync(UiEvents.EmailCampaignCompleted, new { campaign.Id });
                return;
            }

            // Update status to Sending IMMEDIATELY before starting background processing
            campaign.Status = EmailCampaignStatus.Sending;
            campaign.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            logger.LogInformation("Campaign {CampaignId} status set to Sending in database. {Pending} pending recipients.",
                campaignId, pendingRecipients.Count);

            // Publish start event and initial progress IMMEDIATELY
            await uiEventService.PublishAsync(UiEvents.EmailCampaignStarted, new { campaign.Id });
            
            var totalRecipients = campaign.EmailRecipients?.Count ?? 0;
            await PublishProgressAsync(campaignId, totalRecipients, 0);

            // Start background processing WITHOUT awaiting it
            _ = Task.Run(async () => await ProcessEmailsAsync(context, campaign, tokenSource.Token), tokenSource.Token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting campaign {CampaignId}", campaignId);
            campaign.Status = EmailCampaignStatus.Failed;
            campaign.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            
            if (CampaignIds.TryRemove(campaignId, out var cts))
            {
                cts.Dispose();
            }
        }
    }

    public async Task PauseCampaignAsync(Guid campaignId)
    {
        // First update the database status
        await using var context = await contextFactory.CreateDbContextAsync();
        var campaign = await context.EmailCampaigns.FindAsync(campaignId);
        if (campaign != null)
        {
            campaign.Status = EmailCampaignStatus.Paused;
            campaign.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            logger.LogInformation("Campaign {CampaignId} status set to Paused in database", campaignId);
        }
        else
        {
            logger.LogWarning("Campaign {CampaignId} not found during pause.", campaignId);
            return;
        }

        // Then cancel the token to stop processing
        if (!TryCancelCampaign(campaignId))
        {
            logger.LogWarning("No active campaign token found to cancel for {CampaignId}", campaignId);
        }

        await uiEventService.PublishAsync(UiEvents.EmailCampaignPaused, new { Id = campaignId });
    }

    public async Task StopCampaignAsync(Guid campaignId)
    {
        // First update the database status
        await using var context = await contextFactory.CreateDbContextAsync();
        var campaign = await context.EmailCampaigns.FindAsync(campaignId);
        if (campaign != null)
        {
            campaign.Status = EmailCampaignStatus.Cancelled;
            campaign.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            logger.LogInformation("Campaign {CampaignId} status set to Cancelled in database", campaignId);
        }
        else
        {
            logger.LogWarning("Campaign {CampaignId} not found during stop.", campaignId);
            return;
        }

        // Then cancel the token to stop processing
        if (!TryCancelCampaign(campaignId))
        {
            logger.LogWarning("No active campaign token found to cancel for {CampaignId}", campaignId);
        }

        await uiEventService.PublishAsync(UiEvents.EmailCampaignCancelled, new { Id = campaignId });
    }

    private bool TryCancelCampaign(Guid campaignId)
    {
        if (CampaignIds.TryRemove(campaignId, out var tokenSource))
        {
            try
            {
                tokenSource.Cancel();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error cancelling token for {CampaignId}", campaignId);
            }
            finally
            {
                tokenSource.Dispose();
            }
            return true;
        }
        return false;
    }

    private async Task ProcessEmailsAsync(LisaDbContext context, EmailCampaign campaign, CancellationToken cancellationToken)
    {
        try
        {
            var recipients = campaign.EmailRecipients?.ToList() ?? [];
            if (recipients.Count == 0)
            {
                logger.LogWarning("No recipients found for campaign {CampaignId}", campaign.Id);
                campaign.Status = EmailCampaignStatus.Sent;
                await context.SaveChangesAsync(cancellationToken);
                await PublishProgressAsync(campaign.Id, 0, 0, isEmpty: true);
                return;
            }

            var uniqueRecipients = recipients
                .Where(r => r.Status == EmailRecipientStatus.Pending)
                .GroupBy(r => new { r.EmailAddress, r.LearnerId })
                .Select(g => g.First())
                .ToList();

            var total = uniqueRecipients.Count;
            var sent = 0;
            await PublishProgressAsync(campaign.Id, total, sent);

            var batches = uniqueRecipients.Chunk(BatchSize);
            foreach (var batch in batches)
            {
                cancellationToken.ThrowIfCancellationRequested();
                sent = await ProcessBatchAsync(campaign, batch, total, sent, cancellationToken);
            }

            campaign.Status = EmailCampaignStatus.Sent;
            await context.SaveChangesAsync(cancellationToken);
            await uiEventService.PublishAsync(UiEvents.EmailCampaignCompleted, new { campaign.Id });
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Email campaign {CampaignId} was cancelled.", campaign.Id);
            // Don't update status here - it was already set by PauseCampaignAsync or StopCampaignAsync
            var cancelledCampaign = await context.EmailCampaigns.FindAsync(campaign.Id);
            if (cancelledCampaign != null)
            {
                logger.LogInformation("Campaign {CampaignId} status after cancellation: {Status}", campaign.Id, cancelledCampaign.Status);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in ProcessEmailsAsync for campaign {CampaignId}", campaign.Id);
            campaign.Status = EmailCampaignStatus.Failed;
            await context.SaveChangesAsync(CancellationToken.None);
        }
        finally
        {
            // Clean up the CancellationTokenSource when processing completes
            if (CampaignIds.TryRemove(campaign.Id, out var cts))
            {
                cts.Dispose();
            }
        }
    }

    private async Task<int> ProcessBatchAsync(EmailCampaign campaign, IEnumerable<EmailRecipient> batch,
        int total, int processedCount, CancellationToken cancellationToken)
    {
        // Send at 25 emails per minute = 1 email every 2.4 seconds
        const int delayMilliseconds = 2400; // 60000ms / 25 = 2400ms

        var emailRecipients = batch as EmailRecipient[] ?? batch.ToArray();
        foreach (var recipient in emailRecipients)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ProcessRecipientAsync(campaign, recipient, cancellationToken);

            // Add delay between emails (except after the last one in the batch)
            if (recipient != emailRecipients.Last())
            {
                await Task.Delay(delayMilliseconds, cancellationToken);
            }
        }

        processedCount += emailRecipients.Count();

        // Publish summary progress at end of batch
        await PublishBatchProgress(campaign.Id, total, processedCount, cancellationToken);

        return processedCount;
    }

    private async Task ProcessRecipientAsync(EmailCampaign campaign, EmailRecipient recipient, CancellationToken cancellationToken)
    {
        await using var recipientContext = await contextFactory.CreateDbContextAsync();

        try
        {
            if (string.IsNullOrWhiteSpace(recipient.EmailAddress) || !EmailRegex.IsMatch(recipient.EmailAddress))
            {
                logger.LogWarning("Recipient for campaign {CampaignId} has invalid email address: {Email}", campaign.Id, recipient.EmailAddress);
                recipient.Status = EmailRecipientStatus.Bounced;
                recipient.BouncedAt = DateTime.UtcNow;
                recipient.UpdatedAt = DateTime.UtcNow;

                recipientContext.EmailRecipients.Attach(recipient);
                recipientContext.Entry(recipient).Property(r => r.Status).IsModified = true;
                recipientContext.Entry(recipient).Property(r => r.BouncedAt).IsModified = true;
                recipientContext.Entry(recipient).Property(r => r.UpdatedAt).IsModified = true;
                await recipientContext.SaveChangesAsync(cancellationToken);

                // Notify UI immediately
                await NotifyRecipientStatusChange(campaign.Id);
                return;
            }

            var subject = campaign.RecipientTemplate switch
            {
                RecipientTemplate.ProgressFeedback when recipient.LearnerId.HasValue =>
                    await GetSubjectLine(recipient.LearnerId.Value, campaign),
                RecipientTemplate.Newsletter =>
                    GetSubjectLine(campaign),
                RecipientTemplate.Test when recipient.LearnerId.HasValue =>
                    await GetSubjectLine(recipient.LearnerId.Value, campaign),
                _ => throw new InvalidOperationException($"Unsupported recipient template: {campaign.RecipientTemplate}")
            };

            var body = campaign.RecipientTemplate switch
            {
                RecipientTemplate.ProgressFeedback when recipient.LearnerId.HasValue =>
                    await templateRenderService.RenderProgressFeedbackAsync(recipient.LearnerId.Value,
                        campaign.FromDate, campaign.ToDate),
                RecipientTemplate.Test when recipient.LearnerId.HasValue =>
                    await templateRenderService.RenderTestAsync(recipient.LearnerId.Value),
                RecipientTemplate.Newsletter =>
                    await templateRenderService.RenderNewsletterAsync(campaign.SchoolId),
                _ => throw new InvalidOperationException($"Unsupported recipient template: {campaign.RecipientTemplate}")
            };

            if (string.IsNullOrWhiteSpace(body))
            {
                logger.LogError("Failed to render email body for recipient {RecipientEmail} in campaign {CampaignId}", recipient.EmailAddress, campaign.Id);
                recipient.Status = EmailRecipientStatus.Bounced;
                recipient.BouncedAt = DateTime.UtcNow;
                recipient.UpdatedAt = DateTime.UtcNow;

                recipientContext.EmailRecipients.Attach(recipient);
                recipientContext.Entry(recipient).Property(r => r.Status).IsModified = true;
                recipientContext.Entry(recipient).Property(r => r.BouncedAt).IsModified = true;
                recipientContext.Entry(recipient).Property(r => r.UpdatedAt).IsModified = true;
                await recipientContext.SaveChangesAsync(cancellationToken);

                // Notify UI immediately
                await NotifyRecipientStatusChange(campaign.Id);
                return;
            }

            await emailService.SendEmailAsync(recipient.EmailAddress, subject, body, campaign.SchoolId);

            recipient.Status = EmailRecipientStatus.Sent;
            recipient.UpdatedAt = DateTime.UtcNow;

            recipientContext.EmailRecipients.Attach(recipient);
            recipientContext.Entry(recipient).Property(r => r.Status).IsModified = true;
            recipientContext.Entry(recipient).Property(r => r.UpdatedAt).IsModified = true;
            await recipientContext.SaveChangesAsync(cancellationToken);

            // Notify UI immediately
            await NotifyRecipientStatusChange(campaign.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending email to {RecipientEmail}", recipient.EmailAddress);
            recipient.Status = EmailRecipientStatus.Bounced;
            recipient.BouncedAt = DateTime.UtcNow;
            recipient.UpdatedAt = DateTime.UtcNow;

            recipientContext.EmailRecipients.Attach(recipient);
            recipientContext.Entry(recipient).Property(r => r.Status).IsModified = true;
            recipientContext.Entry(recipient).Property(r => r.BouncedAt).IsModified = true;
            recipientContext.Entry(recipient).Property(r => r.UpdatedAt).IsModified = true;
            await recipientContext.SaveChangesAsync(cancellationToken);

            // Notify UI immediately
            await NotifyRecipientStatusChange(campaign.Id);
        }
    }

    private async Task PublishBatchProgress(Guid campaignId, int total, int processedCount, CancellationToken cancellationToken)
    {
        var progress = CalculateProgress(processedCount, total);

        // Get actual counts from database for accuracy
        await using var context = await contextFactory.CreateDbContextAsync();
        var sentCount = await context.EmailRecipients
            .Where(r => r.EmailCampaignId == campaignId && r.Status == EmailRecipientStatus.Sent)
            .CountAsync(cancellationToken);

        var bouncedCount = await context.EmailRecipients
            .Where(r => r.EmailCampaignId == campaignId && r.Status == EmailRecipientStatus.Bounced)
            .CountAsync(cancellationToken);

        await uiEventService.PublishAsync(UiEvents.EmailCampaignProgressUpdated, new
        {
            Id = campaignId,
            Progress = progress,
            Total = total,
            Sent = processedCount,
            ActualSent = sentCount,
            ActualBounced = bouncedCount
        });
    }

    private async Task NotifyRecipientStatusChange(Guid campaignId)
    {
        // Publish a lightweight event to trigger UI refresh
        await uiEventService.PublishAsync(UiEvents.EmailCampaignProgressUpdated, new
        {
            Id = campaignId,
            RecipientStatusChanged = true
        });
    }

    private async Task PublishProgressAsync(Guid campaignId, int total, int sent, bool isEmpty = false)
    {
        var progress = CalculateProgress(sent, total);
        await uiEventService.PublishAsync(UiEvents.EmailCampaignProgressUpdated, new
        {
            Id = campaignId,
            Progress = progress,
            Total = total,
            Sent = sent,
            IsEmpty = isEmpty
        });
    }

    private async Task<List<EmailRecipient>> GenerateRecipientsAsync(CommunicationCommand command)
    {
        Func<CommunicationCommand, Task<List<EmailRecipient>>> strategy = command.RecipientTemplate switch
        {
            RecipientTemplate.ProgressFeedback => GenerateProgressRecipientsAsync,
            RecipientTemplate.Newsletter => GenerateNewsletterRecipientsAsync,
            RecipientTemplate.Test => GenerateProgressRecipientsAsync,
            _ => GenerateNewsletterRecipientsAsync
        };

        var recipients = await strategy(command);

        if (!recipients.Any())
        {
            throw new InvalidOperationException("No valid recipients found for the campaign.");
        }

        return recipients;
    }

    private async Task<List<EmailRecipient>> GenerateProgressRecipientsAsync(CommunicationCommand command)
    {
        var progressRecipients = await GetProgressFeedbackRecipientsAsync(command);
        return progressRecipients.Select(r => new EmailRecipient
        {
            Id = Guid.NewGuid(),
            EmailAddress = r.Email,
            LearnerId = r.LearnerId,
            Status = EmailRecipientStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();
    }

    private async Task<List<EmailRecipient>> GenerateNewsletterRecipientsAsync(CommunicationCommand command)
    {
        var recipientEmails = await GetRecipientEmailsAsync(command);
        return recipientEmails.Select(email => new EmailRecipient
        {
            Id = Guid.NewGuid(),
            EmailAddress = email,
            Status = EmailRecipientStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();
    }

    private async Task<List<(string Email, Guid LearnerId)>> GetProgressFeedbackRecipientsAsync(CommunicationCommand command)
    {
        if (command.RecipientTemplate == RecipientTemplate.ProgressFeedback &&
            (command.FromDate.HasValue || command.ToDate.HasValue))
        {
            var filteredLearners = await GetLearnersWithDateFilterAsync(command);
            return await MapRecipientsAsync(command.RecipientType, filteredLearners, command.SchoolId);
        }
        else
        {
            var learners = await GetLearnersAsync(command);
            return await MapRecipientsAsync(command.RecipientType, learners, command.SchoolId);
        }
    }

    private async Task<List<string>> GetRecipientEmailsAsync(CommunicationCommand command)
    {
        return command.RecipientType switch
        {
            RecipientType.Learner => await GetLearnerEmailsAsync(command),
            RecipientType.Staff => await GetStaffEmailsAsync(command),
            RecipientType.Parent => await GetParentEmailsAsync(command),
            _ => throw new ArgumentException($"Unknown recipient type: {command.RecipientType}", nameof(command.RecipientType))
        };
    }

    private async Task<List<Learner>> GetLearnersAsync(CommunicationCommand command)
    {
        return command.RecipientGroup switch
        {
            RecipientGroup.Learner => await GetLearnerByIdAsync(command),
            RecipientGroup.Subject => await learnerService.GetBySubjectIdAsync(command.SubjectId),
            RecipientGroup.SchoolGrade => await learnerService.GetByGradeAsync(command.GradeId ?? throw new InvalidOperationException("Grade Id cannot be null for SchoolGrade recipient group.")),
            RecipientGroup.School => await learnerService.GetBySchoolAsync(command.SchoolId),
            _ => throw new ArgumentException($"Unknown recipient group: {command.RecipientGroup}", nameof(command.RecipientGroup))
        };
    }

    private async Task<List<Learner>> GetLearnerByIdAsync(CommunicationCommand command)
    {
        if (command.LearnerId == null)
            throw new InvalidOperationException("Learner Id cannot be null for Learner recipient group.");
        var learner = await learnerService.GetByIdAsync(command.LearnerId.Value);
        return learner != null ? [learner] : [];
    }

    private async Task<List<(string Email, Guid LearnerId)>> MapRecipientsAsync(RecipientType recipientType, List<Learner> learners, Guid schoolId)
    {
        List<(string Email, Guid LearnerId)> recipients = [];
        switch (recipientType)
        {
            case RecipientType.Parent:
                foreach (var l in learners)
                {
                    if (l.Parents?.Any() == true)
                    {
                        foreach (var parent in l.Parents)
                        {
                            var hasPrimary = !string.IsNullOrWhiteSpace(parent.PrimaryEmail);
                            var hasSecondary = !string.IsNullOrWhiteSpace(parent.SecondaryEmail);

                            if (hasPrimary)
                            {
                                recipients.Add((parent.PrimaryEmail!, l.Id));
                            }

                            if (hasSecondary)
                            {
                                recipients.Add((parent.SecondaryEmail!, l.Id));
                            }
                        }
                    }
                }
                break;

            case RecipientType.Learner:
                recipients = learners
                    .Where(l => !string.IsNullOrWhiteSpace(l.Email))
                    .Select(l => (l.Email!, l.Id))
                    .ToList();
                break;

            case RecipientType.Staff:
                var roles = new[] { Roles.Administrator, Roles.Principal, Roles.Teacher, Roles.SchoolManagement };
                var staff = await userService.GetAllByRoleAndSchoolAsync(roles, schoolId);
                recipients = staff
                    .Where(s => !string.IsNullOrWhiteSpace(s.Email))
                    .Select(s => (s.Email!, Guid.Empty))
                    .ToList();
                break;
        }

        if (recipients.Count == 0)
        {
            logger.LogWarning("No valid recipients found for {RecipientType} in campaign for SchoolId {SchoolId}", recipientType, schoolId);
        }
        return recipients;
    }

    private async Task<List<string>> GetLearnerEmailsAsync(CommunicationCommand command)
    {
        var learners = await learnerService.GetBySchoolAsync(command.SchoolId);
        return learners
            .Where(l => !string.IsNullOrWhiteSpace(l.Email))
            .Select(l => l.Email!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<List<string>> GetParentEmailsAsync(CommunicationCommand command)
    {
        var learners = await learnerService.GetLearnersBySchoolWithParentsAsync(command.SchoolId);
        return learners
            .Where(l => l.Parents != null && l.Parents.Any())
            .SelectMany(l => l.Parents!)
            .SelectMany(p => new[] { p.PrimaryEmail, p.SecondaryEmail })
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(email => email!)
            .ToList();
    }

    private async Task<List<string>> GetStaffEmailsAsync(CommunicationCommand command)
    {
        var roles = new[] { Roles.Administrator, Roles.Principal, Roles.Teacher, Roles.SchoolManagement };
        var staff = await userService.GetAllByRoleAndSchoolAsync(roles, command.SchoolId);
        return staff
            .Where(s => !string.IsNullOrWhiteSpace(s.Email))
            .Select(s => s.Email!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private bool ValidateCampaign(EmailCampaign campaign)
    {
        if (string.IsNullOrWhiteSpace(campaign.Name))
        {
            logger.LogWarning("Campaign {CampaignId} has an invalid name.", campaign.Id);
            return false;
        }
        if (campaign.EmailRecipients == null || !campaign.EmailRecipients.Any())
        {
            logger.LogWarning("Campaign {CampaignId} has no recipients.", campaign.Id);
            return false;
        }
        return true;
    }

    private static string GenerateCampaignName(CommunicationCommand command)
    {
        return $"{command.RecipientGroup} - {command.RecipientType} - {command.RecipientTemplate} - {DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    private static string GetSubjectLine(EmailCampaign command)
    {
        if (command.RecipientTemplate == RecipientTemplate.Newsletter)
        {
            return "Latest Newsletter";
        }
        else if (command.RecipientTemplate == RecipientTemplate.Test)
        {
            return "Progress Feedback Confirmation Email";
        }
        else
        {
            return "Important Update from Your School";
        }
    }

    private async Task<string> GetSubjectLine(Guid learnerId, EmailCampaign command)
    {
        var learner = await learnerService.GetByIdAsync(learnerId);
        if (command.RecipientTemplate == RecipientTemplate.Test)
        {
            return $"Progress Feedback Confirmation - {learner?.Name} {learner?.Surname}";
        }
        else
        {
            return $"Progress Feedback - {learner?.Name} {learner?.Surname}";
        }
    }

    private static int CalculateProgress(int processedCount, int total)
    {
        return total > 0 ? (int)((double)processedCount / total * 100) : ProgressComplete;
    }

    private async Task<List<Learner>> GetLearnersWithDateFilterAsync(CommunicationCommand command)
    {
        if (command.RecipientGroup == RecipientGroup.Learner && command.LearnerId.HasValue)
        {
            var learner = await learnerService.GetByIdAsync(command.LearnerId.Value);
            return learner != null ? [learner] : [];
        }

        await using var context = await contextFactory.CreateDbContextAsync();
        var progressService = progressFeedbackService;

        DateTime? fromDateUtc = command.FromDate.HasValue ? command.FromDate.Value : null;
        DateTime? toDateUtc = command.ToDate.HasValue ? command.ToDate.Value : null;

        var progressItems = await progressService.GetProgressFeedbackListAsync(
            command.SchoolId,
            command.RecipientGroup == RecipientGroup.SchoolGrade ? command.GradeId : null,
            command.RecipientGroup == RecipientGroup.Subject ? command.SubjectId : null,
            fromDateUtc,
            toDateUtc
        );

        List<Learner> learners = [];
        foreach (var item in progressItems)
        {
            var learner = await learnerService.GetByIdAsync(item.LearnerId);
            if (learner != null)
            {
                learners.Add(learner);
            }
        }

        return learners;
    }
}