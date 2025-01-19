using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;


public class EmailTemplateService(IDbContextFactory<LisaDbContext> contextFactory)
{
    private readonly IDbContextFactory<LisaDbContext> _contextFactory = contextFactory;

    public async Task<List<EmailTemplate>> GetAllTemplatesAsync()
    {
        var _context = await _contextFactory.CreateDbContextAsync();
        return await _context.EmailTemplates
            .OrderByDescending(t => t.UpdatedAt).ToListAsync();
    }

    public async Task<EmailTemplate?> GetTemplateByIdAsync(Guid id)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.EmailTemplates.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task DeleteTemplateAsync(Guid id)
    {
        var _context = await _contextFactory.CreateDbContextAsync();
        var template = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Id == id);
        if (template != null)
        {
            _context.EmailTemplates.Remove(template);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SaveTemplateAsync(string name, string subject, string content)
    {
        var _context = await _contextFactory.CreateDbContextAsync();
        var template = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Name == name);

        if (template == null)
        {
            template = new EmailTemplate
            {
                Id = Guid.NewGuid(),
                Name = name,
                Subject = subject,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.EmailTemplates.Add(template);
        }
        else
        {
            template.Subject = subject;
            template.Content = content;
            template.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<EmailTemplate?> GetTemplateAsync(string name)
    {
        var _context = await _contextFactory.CreateDbContextAsync();
        var template = await _context.EmailTemplates.FirstOrDefaultAsync(t => t.Name == name);
        await _context.SaveChangesAsync();
        return template;
    }

    public async Task<bool> UpdateTemplateAsync(EmailTemplate template)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var existingTemplate = await context.EmailTemplates.FirstOrDefaultAsync(t => t.Id == template.Id);

        if (existingTemplate != null)
        {
            existingTemplate.Name = template.Name;
            existingTemplate.Subject = template.Subject;
            existingTemplate.Content = template.Content;

            await context.SaveChangesAsync();
            return true;
        }

        return false;
    }
}
