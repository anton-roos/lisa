using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class BugReportService
(
    IHttpContextAccessor httpContextAccessor,
    NavigationManager navigationManager,
    VersionService versionService,
    IDbContextFactory<LisaDbContext> dbContextFactory,
    EmailService emailService,
    ILogger<BugReportService> logger,
    SchoolService schoolService
)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly NavigationManager _navigationManager = navigationManager;
    private readonly VersionService _versionService = versionService;
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly EmailService _emailService = emailService;
    private readonly ILogger<BugReportService> _logger = logger;
    private readonly SchoolService _schoolService = schoolService;

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
            await SendBugReportEmailAsync(bugReport);
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

    public async Task SendBugReportEmailAsync(BugReport bugReport)
    {
        try
        {
            var selectedSchool = await _schoolService.GetSelectedSchoolAsync();
            var htmlBody = $@"
                    <!DOCTYPE html>
                    <html lang=""en"">
                    <head>
                        <meta charset=""UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
                        <title>Bug Report</title>
                        <style>
                            body {{ font-family: Arial, sans-serif; background-color: #f9f9f9; padding: 20px; }}
                            .container {{ max-width: 600px; margin: auto; background: #fff; border: 1px solid #ddd; padding: 20px; border-radius: 8px; }}
                            .header {{ padding: 10px; text-align: center; }}
                            .footer {{ text-align: center; font-size: 12px; color: #666; padding: 10px; }}
                        </style>
                    </head>
                    <body>
                        <div class=""container"">
                            <div class=""header""><h1>Bug Report</h1></div>
                            <p><strong>Reported At:</strong> {bugReport.ReportedAt}</p>
                            <p><strong>Reported By:</strong> {bugReport.ReportedBy ?? "Anonymous"}</p>
                            <p><strong>User Authenticated:</strong> {(bugReport.UserAuthenticated ? "Yes" : "No")}</p>
                            <p><strong>Page URL:</strong> <a href=""{bugReport.PageUrl}"">{bugReport.PageUrl}</a></p>
                            <p><strong>App Version:</strong> {bugReport.Version}</p>
                            <hr>
                            <p><strong>What Happened:</strong> {bugReport.WhatHappened}</p>
                            <p><strong>What Was Tried:</strong> {bugReport.WhatTried}</p>
                        </div>
                    </body>
                    </html>
                ";
            
            await _emailService.SendEmailAsync(
                "antonroos992@gmail.com",
                "Bug Report",
                htmlBody,
                selectedSchool?.Id ?? Guid.Empty
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bug report email: {Message}", ex.Message);
        }
    }
}
