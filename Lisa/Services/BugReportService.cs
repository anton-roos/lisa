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
}