using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public interface IEmailTemplateService
{
    Task SaveTemplateAsync(string name, string subject, string content);
    Task<EmailTemplate?> GetTemplateAsync(string name);
}

public class EmailTemplateService : IEmailTemplateService
{
    private readonly IDbContextFactory<LisaDbContext> _contextFactory;

    public EmailTemplateService(IDbContextFactory<LisaDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
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
}
