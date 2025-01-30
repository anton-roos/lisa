using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace Lisa.Services
{
    public class EmailCampaignService
    {
        private readonly DbContextFactory<LisaDbContext> _contextFactory;

        public EmailCampaignService(DbContextFactory<LisaDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Get a list of all EmailCampaigns.
        /// </summary>
        public async Task<List<EmailCampaign>> GetAllAsync()
        {
            var context = await _contextFactory.CreateDbContextAsync();
            return await context.EmailCampaigns
                .AsNoTracking()
                .ToListAsync();
        }

        /// <summary>
        /// Get a single EmailCampaign by Id.
        /// </summary>
        public async Task<EmailCampaign?> GetByIdAsync(Guid? id)
        {
            if (id == null) return null;

            var context = await _contextFactory.CreateDbContextAsync();
            return await context.EmailCampaigns
                .Include(ec => ec.EmailRecipients) // Optional: if you want to load recipients too
                .FirstOrDefaultAsync(ec => ec.Id == id);
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

            var context = await _contextFactory.CreateDbContextAsync();

            context.EmailCampaigns.Add(campaign);
            await context.SaveChangesAsync();

            return campaign;
        }

        /// <summary>
        /// Update an existing EmailCampaign.
        /// </summary>
        public async Task UpdateAsync(EmailCampaign campaign)
        {
            var context = await _contextFactory.CreateDbContextAsync();

            var existing = await context.EmailCampaigns.FindAsync(campaign.Id);
            if (existing == null) return;

            // Update fields as needed
            existing.Name = campaign.Name;
            existing.Description = campaign.Description;
            existing.SubjectLine = campaign.SubjectLine;
            existing.SenderName = campaign.SenderName;
            existing.SenderEmail = campaign.SenderEmail;
            existing.ContentHtml = campaign.ContentHtml;
            existing.ContentText = campaign.ContentText;
            existing.Status = campaign.Status;
            existing.ScheduledAt = campaign.ScheduledAt;
            existing.CompletedAt = campaign.CompletedAt;
            existing.TrackOpens = campaign.TrackOpens;
            existing.TrackClicks = campaign.TrackClicks;
            existing.StatsSentCount = campaign.StatsSentCount;
            existing.StatsOpenCount = campaign.StatsOpenCount;
            existing.StatsClickCount = campaign.StatsClickCount;
            existing.UpdatedAt = DateTime.UtcNow;

            // EF tracks the changes automatically. Just save them.
            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Delete an EmailCampaign by Id.
        /// </summary>
        public async Task DeleteAsync(Guid id)
        {
            var context = await _contextFactory.CreateDbContextAsync();

            var existing = await context.EmailCampaigns.FindAsync(id);
            if (existing == null) return;

            context.EmailCampaigns.Remove(existing);
            await context.SaveChangesAsync();
        }
    }
}
