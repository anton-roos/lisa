using Lisa.Data;
using Lisa.Enums;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class EmailCampaignBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<EmailCampaignBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Email Campaign Background Service is starting.");

        // Wait a bit for the app to fully initialize
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

        await ResumeIncompleteCampaignsAsync(stoppingToken);

        // Periodically check for stuck campaigns every 5 minutes
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await CheckForStuckCampaignsAsync(stoppingToken);
        }
    }

    private async Task ResumeIncompleteCampaignsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<LisaDbContext>>();
            var campaignService = scope.ServiceProvider.GetRequiredService<EmailCampaignService>();

            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            // Find campaigns that were sending or paused when the app stopped
            var incompleteCampaigns = await context.EmailCampaigns
                .Where(c => c.Status == EmailCampaignStatus.Sending || c.Status == EmailCampaignStatus.Paused)
                .ToListAsync(cancellationToken);

            if (incompleteCampaigns.Any())
            {
                logger.LogInformation("Found {Count} incomplete campaigns to resume.", incompleteCampaigns.Count);

                foreach (var campaign in incompleteCampaigns)
                {
                    try
                    {
                        if (campaign.Status == EmailCampaignStatus.Sending)
                        {
                            logger.LogInformation("Resuming campaign {CampaignId} ({CampaignName}) that was in Sending state", 
                                campaign.Id, campaign.Name);
                            
                            // Resume the campaign
                            await campaignService.StartCampaignAsync(campaign.Id);
                        }
                        else if (campaign.Status == EmailCampaignStatus.Paused)
                        {
                            logger.LogInformation("Campaign {CampaignId} ({CampaignName}) remains in Paused state - can be resumed manually", 
                                campaign.Id, campaign.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error resuming campaign {CampaignId}", campaign.Id);
                    }
                }
            }
            else
            {
                logger.LogInformation("No incomplete campaigns found to resume.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking for incomplete campaigns on startup");
        }
    }

    private async Task CheckForStuckCampaignsAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<LisaDbContext>>();

            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

            // Find campaigns that have been sending for more than 2 hours without completion
            var stuckThreshold = DateTime.UtcNow.AddHours(-2);
            var potentiallyStuck = await context.EmailCampaigns
                .Include(c => c.EmailRecipients)
                .Where(c => c.Status == EmailCampaignStatus.Sending && c.UpdatedAt < stuckThreshold)
                .ToListAsync(cancellationToken);

            foreach (var campaign in potentiallyStuck)
            {
                var totalRecipients = campaign.EmailRecipients?.Count ?? 0;
                var pendingRecipients = campaign.EmailRecipients?.Count(r => r.Status == EmailRecipientStatus.Pending) ?? 0;
                var processedRecipients = totalRecipients - pendingRecipients;

                logger.LogWarning(
                    "Campaign {CampaignId} ({CampaignName}) may be stuck. Status: {Status}, Last Updated: {UpdatedAt}, Processed: {Processed}/{Total}",
                    campaign.Id, campaign.Name, campaign.Status, campaign.UpdatedAt, processedRecipients, totalRecipients);

                // If all recipients are processed, mark as complete
                if (pendingRecipients == 0 && totalRecipients > 0)
                {
                    logger.LogInformation("Marking stuck campaign {CampaignId} as Sent - all recipients processed", campaign.Id);
                    campaign.Status = EmailCampaignStatus.Sent;
                    campaign.UpdatedAt = DateTime.UtcNow;
                    await context.SaveChangesAsync(cancellationToken);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking for stuck campaigns");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Email Campaign Background Service is stopping.");
        await base.StopAsync(cancellationToken);
    }
}
