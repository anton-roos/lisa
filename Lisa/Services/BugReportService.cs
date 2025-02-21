using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class BugReportService(
    IHttpContextAccessor httpContextAccessor,
    NavigationManager navigationManager,
    VersionService versionService,
    IDbContextFactory<LisaDbContext> dbContextFactory,
    EmailService emailService,
    ILogger<BugReportService> logger)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly NavigationManager _navigationManager = navigationManager;
    private readonly VersionService _versionService = versionService;
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly EmailService _emailService = emailService;
    private readonly ILogger<BugReportService> _logger = logger;

    public async Task<List<BugReport>> GetAllAsync()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.BugReports
            .AsNoTracking()
            .ToListAsync();
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

        bugReport.PageUrl = _navigationManager.Uri;
        bugReport.Version = _versionService.GetVersion();

        using var context = await _dbContextFactory.CreateDbContextAsync();
        await context.BugReports.AddAsync(bugReport);
        await context.SaveChangesAsync();

        try
        {
            await _emailService.SendBugReportEmailAsync(bugReport);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to send bug report email: {exceptionMessage}", ex.Message);
        }
    }

    public async Task UpdateStatusAsync(Guid id, BugReportStatus status)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        var bugReport = await context.BugReports.FindAsync(id);
        if (bugReport == null)
        {
            _logger.LogWarning("Bug report with ID {id} not found.", id);
            return;
        }

        bugReport.Status = status;

        if (bugReport.ResolvedAt == null && status == BugReportStatus.Resolved)
        {
            bugReport.ResolvedAt = DateTime.UtcNow;
        }

        if (bugReport.ClosedAt == null && status == BugReportStatus.Closed)
        {
            bugReport.ClosedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();
    }

    public async Task<int> GetCountAsync()
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.BugReports.CountAsync();
    }

    public async Task<BugReport?> GetAsync(Guid id)
    {
        using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.BugReports
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }
}
