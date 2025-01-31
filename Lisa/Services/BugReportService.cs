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

    /// <summary>
    /// Retrieve all bug reports.
    /// </summary>
    public async Task<List<BugReport>> GetAllAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.BugReports
            .AsNoTracking() // Optimization for read-only queries
            .ToListAsync();
    }

    /// <summary>
    /// Log a new bug report.
    /// </summary>
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

        await using var context = await _dbContextFactory.CreateDbContextAsync();
        await context.BugReports.AddAsync(bugReport);
        await context.SaveChangesAsync();

        try
        {
            await _emailService.SendBugReportEmailAsync(bugReport);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send bug report email: {ex.Message}");
        }
    }

    /// <summary>
    /// Update the status of a bug report.
    /// </summary>
    public async Task UpdateStatusAsync(Guid id, BugReportStatus status)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var bugReport = await context.BugReports.FindAsync(id);
        if (bugReport == null)
        {
            _logger.LogWarning($"Bug report with ID {id} not found.");
            return;
        }

        bugReport.Status = status;
        switch (status)
        {
            case BugReportStatus.Resolved:
                bugReport.ResolvedAt = DateTime.UtcNow;
                break;
            case BugReportStatus.Closed:
                bugReport.ClosedAt = DateTime.UtcNow;
                break;
        }

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Get the total count of bug reports.
    /// </summary>
    public async Task<int> GetCountAsync()
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.BugReports.CountAsync();
    }

    /// <summary>
    /// Delete a bug report by ID.
    /// </summary>
    public async Task DeleteBugAsync(Guid id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        var bugReport = await context.BugReports.FindAsync(id);
        if (bugReport == null)
        {
            _logger.LogWarning($"Bug report with ID {id} not found.");
            return;
        }

        context.BugReports.Remove(bugReport);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Retrieve a specific bug report by ID.
    /// </summary>
    public async Task<BugReport?> GetAsync(Guid id)
    {
        await using var context = await _dbContextFactory.CreateDbContextAsync();
        return await context.BugReports
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }
}
