using System.Collections.Concurrent;
using Hangfire;
using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class EmailCampaignService
(
    IDbContextFactory<LisaDbContext> contextFactory,
    IUiEventService uiEventService,
    ILogger<EmailCampaignService> logger,
    EmailService emailService
)
{
    private readonly IDbContextFactory<LisaDbContext> _contextFactory = contextFactory;
    private readonly IUiEventService _uiEventService = uiEventService;
    private readonly ILogger<EmailCampaignService> _logger = logger;
    private static readonly ConcurrentDictionary<Guid, CancellationTokenSource> _campaignTokens = new();
    private readonly EmailService _emailService = emailService;

    /// <summary>
    /// Schedules an email campaign using Hangfire.
    /// </summary>
    public async Task ScheduleCampaignAsync(Guid campaignId)
    {
        var campaign = await GetByIdAsync(campaignId);
        if (campaign == null || campaign.Status == EmailCampaignStatus.Sent)
        {
            _logger.LogWarning("Campaign {campaignId} not found or already sent.", campaignId);
            return;
        }

        _logger.LogInformation("Scheduling email campaign {campaignId} using Hangfire.", campaignId);
        BackgroundJob.Enqueue(() => StartCampaignAsync(campaignId));
    }

    /// <summary>
    /// Get a list of all EmailCampaigns.
    /// </summary>
    public async Task<List<EmailCampaign>> GetAllAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.EmailCampaigns
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
            await using var context = await _contextFactory.CreateDbContextAsync();
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

            await _uiEventService.PublishAsync(UiEvents.EmailCampaignStarted, new { Id = campaign.Id });

            var tokenSource = new CancellationTokenSource();
            if (!_campaignTokens.TryAdd(campaignId, tokenSource))
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
            await using var context = await _contextFactory.CreateDbContextAsync();
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

                        BackgroundJob.Enqueue(() => SendEmailWithRetryAsync(recipient.EmailAddress, subject, body, campaign.SchoolId));
                        Thread.Sleep(2000);

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
                        Id = campaign.Id,
                        Progress = progress,
                        Total = total,
                        Sent = sent
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error sending email to {recipient.EmailAddress}: {ex.Message}", recipient.EmailAddress, ex.Message);
                    recipient.Status = EmailRecipientStatus.Bounced;
                }
            }

            campaign.Status = EmailCampaignStatus.Sent;
            await context.SaveChangesAsync(cancellationToken);

            await _uiEventService.PublishAsync(UiEvents.EmailCampaignCompleted, new { Id = campaign.Id });
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Email campaign {campaignId} was cancelled.", campaignId);
        }
        catch (Exception ex)
        {
            _logger.LogError("Unexpected error in ProcessEmailsAsync for campaign {campaignId}: {ex.Message}", campaignId, ex.Message);
        }
        finally
        {
            _campaignTokens.TryRemove(campaignId, out _);
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
                await _emailService.SendEmailAsync(schoolId: schoolId, to: recipientEmailAddress, subject: subject, body: body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to send email to {recipient.EmailAddress}: {ex.Message}", recipientEmailAddress, ex.Message);
            throw;
        }
    }

    public async Task PauseCampaignAsync(Guid campaignId)
    {
        if (!TryCancelCampaign(campaignId))
            return;

        await using var context = await _contextFactory.CreateDbContextAsync();
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

        await using var context = await _contextFactory.CreateDbContextAsync();
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
        if (_campaignTokens.TryGetValue(campaignId, out var tokenSource))
        {
            tokenSource.Cancel();
            _campaignTokens.TryRemove(campaignId, out _);
            return true;
        }

        _logger.LogWarning("Attempted to cancel campaign {campaignId}, but no active task was found.", campaignId);
        return false;
    }

    public async Task<EmailCampaign?> GetByIdAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.EmailCampaigns.FindAsync(id);
    }

    /// <summary>
    /// Create a new EmailCampaign.
    /// </summary>
    public async Task<EmailCampaign> CreateAsync(EmailCampaign campaign)
    {
        if (campaign.Id == null || campaign.Id == Guid.Empty)
        {
            campaign.Id = Guid.NewGuid(); // ensure there's a GUID
        }

        campaign.CreatedAt = DateTime.UtcNow;
        campaign.UpdatedAt = DateTime.UtcNow;

        await using var context = await _contextFactory.CreateDbContextAsync();

        context.EmailCampaigns.Add(campaign);
        await context.SaveChangesAsync();

        return campaign;
    }
}
