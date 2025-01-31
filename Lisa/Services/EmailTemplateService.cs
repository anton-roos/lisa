using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class EmailTemplateService(IDbContextFactory<LisaDbContext> contextFactory, ILogger<EmailTemplateService> logger)
{
    private readonly IDbContextFactory<LisaDbContext> _contextFactory = contextFactory;
    private readonly ILogger<EmailTemplateService> _logger = logger;

    /// <summary>
    /// Retrieves all email templates ordered by the most recent update.
    /// </summary>
    public async Task<List<EmailTemplate>> GetAllTemplatesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.EmailTemplates
            .AsNoTracking()
            .OrderByDescending(t => t.UpdatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves an email template by its unique ID.
    /// </summary>
    public async Task<EmailTemplate?> GetTemplateByIdAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.EmailTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    /// <summary>
    /// Deletes an email template by its unique ID.
    /// </summary>
    public async Task<bool> DeleteTemplateAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var template = await context.EmailTemplates.FindAsync(id);
        if (template == null)
        {
            _logger.LogWarning("Attempted to delete template {id}, but it does not exist.", id);
            return false;
        }

        context.EmailTemplates.Remove(template);
        await context.SaveChangesAsync();
        _logger.LogInformation("Successfully deleted template {id}.", id);
        return true;
    }

    /// <summary>
    /// Saves a new template or updates an existing one by name.
    /// </summary>
    public async Task<bool> SaveTemplateAsync(string name, string subject, string content)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var existingTemplate = await context.EmailTemplates.FirstOrDefaultAsync(t => t.Name == name);

            if (existingTemplate == null)
            {
                var newTemplate = new EmailTemplate
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    Subject = subject,
                    Content = content,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await context.EmailTemplates.AddAsync(newTemplate);
                _logger.LogInformation("Created new template '{name}'.", name);

            }
            else
            {
                existingTemplate.Subject = subject;
                existingTemplate.Content = content;
                existingTemplate.UpdatedAt = DateTime.UtcNow;

                context.Entry(existingTemplate).State = EntityState.Modified;
                _logger.LogInformation("Updated template '{name}'.", name);
            }

            await context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error saving template '{name}': {ex.Message}", name, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Retrieves an email template by its name.
    /// </summary>
    public async Task<EmailTemplate?> GetTemplateAsync(string name)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.EmailTemplates
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Name == name);
    }

    /// <summary>
    /// Updates an existing template by ID.
    /// </summary>
    public async Task<bool> UpdateTemplateAsync(EmailTemplate template)
    {
        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync();
            var existingTemplate = await context.EmailTemplates.FindAsync(template.Id);

            if (existingTemplate == null)
            {
                _logger.LogWarning("Attempted to update template {template.Id}, but it does not exist.", template.Id);
                return false;
            }

            existingTemplate.Name = template.Name;
            existingTemplate.Subject = template.Subject;
            existingTemplate.Content = template.Content;
            existingTemplate.UpdatedAt = DateTime.UtcNow;

            context.Entry(existingTemplate).State = EntityState.Modified;
            await context.SaveChangesAsync();

            _logger.LogInformation("Successfully updated template {template.Id}.", template.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error updating template {template.Id}: {ex.Message}", template.Id, ex.Message);
            return false;
        }
    }
}
