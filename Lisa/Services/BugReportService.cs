using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;


public class BugReportService(IHttpContextAccessor httpContextAccessor, NavigationManager navigationManager, VersionService versionService, IDbContextFactory<LisaDbContext> dbContextFactory, EmailService emailService)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly NavigationManager _navigationManager = navigationManager;
    private readonly VersionService _versionService = versionService;
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly EmailService _emailService = emailService;

    public async Task<List<BugReport>> GetAllAsync()
    {
        await using var context = _dbContextFactory.CreateDbContext();
        return await context.BugReports.ToListAsync();
    }
    
    public async Task LogBugAsync(BugReport bugReport)
    {
        bugReport.ReportedAt = DateTime.UtcNow;

        var user = _httpContextAccessor.HttpContext?.User;
        if (user != null)
        {
            bugReport.UserAuthenticated = user.Identity?.IsAuthenticated == true;
            bugReport.ReportedBy = user.Identity?.Name;
        }

        if (_navigationManager.Uri != null)
        {
            bugReport.PageUrl = _navigationManager.Uri;
        }

        var version = _versionService.GetVersion();

        if (version != null)
        {
            bugReport.Version = version;
        }

        var context = _dbContextFactory.CreateDbContext();
        await context.BugReports.AddAsync(bugReport);
        await context.SaveChangesAsync();

        await _emailService.SendBugReportEmailAsync(bugReport);
    }
    
    public async Task UpdateStatusAsync(Guid id, BugReportStatus status)
    {
        await using var context = _dbContextFactory.CreateDbContext();
        var bugReport = await context.BugReports.FindAsync(id);
        if (bugReport == null) return;

        bugReport.Status = status;

        switch (status)
        {
            case BugReportStatus.Resolved:
                bugReport.ResolvedAt = DateTime.UtcNow;
                break;
            case BugReportStatus.Closed:
                context.BugReports.Remove(bugReport);
                break;
        }
        
        await context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync()
    {
        await using var context = _dbContextFactory.CreateDbContext();
        return await context.BugReports.CountAsync();
    }

    public async Task DeleteBugAsync(Guid id)
    {
        var context = _dbContextFactory.CreateDbContext();
        var bugReport = await context.BugReports.FindAsync(id);
        if (bugReport == null) return;

        context.BugReports.Remove(bugReport);
        await context.SaveChangesAsync();
    }

    public async Task<BugReport> GetAsync(Guid id)
    {
        await using var context = _dbContextFactory.CreateDbContext();
        return await context.BugReports.FindAsync(id);
    }
}