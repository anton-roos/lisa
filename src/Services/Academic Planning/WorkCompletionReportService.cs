using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lisa.Data;
using Lisa.Models.AcademicPlanning;
using Lisa.Models.Entities;
using Lisa.Services;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services.AcademicPlanning
{
    public class WorkCompletionReportService : IWorkCompletionReportService
    {
        private readonly IDbContextFactory<LisaDbContext> _dbFactory;
        private readonly EmailService _emailService;
        private readonly UserService _userService;

        public WorkCompletionReportService(IDbContextFactory<LisaDbContext> dbFactory, EmailService emailService, UserService userService)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
            _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<WorkCompletionReport> GenerateReportAsync(Guid teachingPlanId, DateTime periodStartDate, DateTime periodEndDate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var plan = await db.Set<TeachingPlan>()
                .Include(p => p.Weeks)
                    .ThenInclude(w => w.Periods)
                .FirstOrDefaultAsync(p => p.Id == teachingPlanId, cancellationToken);

            if (plan == null)
            {
                throw new ArgumentException("Teaching plan not found");
            }

            // Get all periods within the date range
            var periods = plan.Weeks
                .SelectMany(w => w.Periods ?? Enumerable.Empty<AcademicPlanPeriod>())
                .Where(p => p.DatePlanned.HasValue && 
                           p.DatePlanned.Value >= periodStartDate && 
                           p.DatePlanned.Value <= periodEndDate)
                .OrderBy(p => p.DatePlanned)
                .ToList();

            var report = new WorkCompletionReport
            {
                Id = Guid.NewGuid(),
                SchoolId = plan.SchoolId,
                TeachingPlanId = teachingPlanId,
                ReportDate = DateTime.UtcNow,
                PeriodStartDate = periodStartDate,
                PeriodEndDate = periodEndDate,
                TotalPeriods = periods.Count,
                PlannedPeriods = periods.Count(p => p.DatePlanned.HasValue),
                CompletedPeriods = periods.Count(p => p.DateCompleted.HasValue),
                AveragePercentagePlanned = periods.Where(p => p.PercentagePlanned.HasValue).Any() 
                    ? (decimal)periods.Where(p => p.PercentagePlanned.HasValue).Average(p => (double)p.PercentagePlanned!.Value) 
                    : 0,
                AveragePercentageCompleted = periods.Where(p => p.PercentageCompleted.HasValue).Any()
                    ? (decimal)periods.Where(p => p.PercentageCompleted.HasValue).Average(p => (double)p.PercentageCompleted!.Value)
                    : 0,
                PeriodsBehindSchedule = periods.Count(p => p.DateCompleted.HasValue && p.DatePlanned.HasValue && p.DateCompleted.Value > p.DatePlanned.Value),
                CreatedAt = DateTime.UtcNow
            };

            // Create details
            foreach (var period in periods)
            {
                var week = plan.Weeks.FirstOrDefault(w => w.Periods?.Any(p => p.Id == period.Id) ?? false);
                var detail = new WorkCompletionReportDetail
                {
                    Id = Guid.NewGuid(),
                    WorkCompletionReportId = report.Id,
                    AcademicPlanPeriodId = period.Id,
                    WeekNumber = week?.WeekNumber ?? 0,
                    PeriodNumber = period.PeriodNumber,
                    Topic = period.Topic,
                    SubTopic = period.SubTopic,
                    DatePlanned = period.DatePlanned,
                    PercentagePlanned = period.PercentagePlanned,
                    DateCompleted = period.DateCompleted,
                    PercentageCompleted = period.PercentageCompleted,
                    IsOnSchedule = !period.DateCompleted.HasValue || !period.DatePlanned.HasValue || period.DateCompleted.Value <= period.DatePlanned.Value,
                    IsLate = period.DateCompleted.HasValue && period.DatePlanned.HasValue && period.DateCompleted.Value > period.DatePlanned.Value,
                    DaysBehind = period.DateCompleted.HasValue && period.DatePlanned.HasValue && period.DateCompleted.Value > period.DatePlanned.Value
                        ? (int?)(period.DateCompleted.Value - period.DatePlanned.Value).TotalDays
                        : null
                };

                report.Details.Add(detail);
            }

            await db.Set<WorkCompletionReport>().AddAsync(report, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            return report;
        }

        public async Task<List<WorkCompletionReport>> GenerateReportsForSchoolAsync(Guid schoolId, DateTime periodStartDate, DateTime periodEndDate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var plans = await db.Set<TeachingPlan>()
                .Where(p => p.SchoolId == schoolId)
                .ToListAsync(cancellationToken);

            var reports = new List<WorkCompletionReport>();

            foreach (var plan in plans)
            {
                var report = await GenerateReportAsync(plan.Id, periodStartDate, periodEndDate, cancellationToken);
                reports.Add(report);
            }

            return reports;
        }

        public async Task<WorkCompletionReport?> GetReportAsync(Guid reportId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<WorkCompletionReport>()
                .AsNoTracking()
                .Include(r => r.Details)
                    .ThenInclude(d => d.AcademicPlanPeriod)
                .Include(r => r.TeachingPlan)
                    .ThenInclude(p => p!.Weeks)
                .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);
        }

        public async Task<List<WorkCompletionReport>> GetReportsByTeachingPlanAsync(Guid teachingPlanId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<WorkCompletionReport>()
                .AsNoTracking()
                .Include(r => r.Details)
                .Where(r => r.TeachingPlanId == teachingPlanId)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<WorkCompletionReport>> GetReportsBySchoolAsync(Guid schoolId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<WorkCompletionReport>()
                .AsNoTracking()
                .Include(r => r.Details)
                .Where(r => r.SchoolId == schoolId)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<WorkCompletionReportRecipient> AddRecipientAsync(Guid schoolId, Guid userId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.Set<WorkCompletionReportRecipient>()
                .FirstOrDefaultAsync(r => r.SchoolId == schoolId && r.UserId == userId, cancellationToken);

            if (existing != null)
            {
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
                return existing;
            }

            var recipient = new WorkCompletionReportRecipient
            {
                Id = Guid.NewGuid(),
                SchoolId = schoolId,
                UserId = userId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await db.Set<WorkCompletionReportRecipient>().AddAsync(recipient, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);

            return recipient;
        }

        public async Task<bool> RemoveRecipientAsync(Guid recipientId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var recipient = await db.Set<WorkCompletionReportRecipient>().FindAsync(new object[] { recipientId }, cancellationToken);
            if (recipient == null) return false;

            recipient.IsActive = false;
            recipient.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<WorkCompletionReportRecipient>> GetRecipientsBySchoolAsync(Guid schoolId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<WorkCompletionReportRecipient>()
                .AsNoTracking()
                .Include(r => r.User)
                .Where(r => r.SchoolId == schoolId && r.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<User>> GetRecipientsForSchoolAsync(Guid schoolId, CancellationToken cancellationToken = default)
        {
            var recipients = await GetRecipientsBySchoolAsync(schoolId, cancellationToken);
            return recipients.Select(r => r.User!).Where(u => u != null).ToList();
        }

        public async Task SendWeeklyReportsAsync(Guid schoolId, CancellationToken cancellationToken = default)
        {
            // Calculate date range for last week
            var today = DateTime.UtcNow.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(6);

            // Generate reports for all teaching plans in the school
            var reports = await GenerateReportsForSchoolAsync(schoolId, startOfWeek, endOfWeek, cancellationToken);

            // Get recipients
            var recipients = await GetRecipientsForSchoolAsync(schoolId, cancellationToken);
            var recipientIds = recipients.Select(r => r.Id).ToList();

            // Send each report
            foreach (var report in reports)
            {
                await SendReportAsync(report, recipientIds, cancellationToken);
            }
        }

        public async Task SendReportAsync(WorkCompletionReport report, List<Guid> recipientUserIds, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var recipients = await db.Users
                .Where(u => recipientUserIds.Contains(u.Id))
                .ToListAsync(cancellationToken);

            foreach (var recipient in recipients)
            {
                if (string.IsNullOrWhiteSpace(recipient.Email))
                    continue;

                var subject = $"Work Completion Report - {report.ReportDate:yyyy-MM-dd}";
                var body = await GenerateReportEmailBodyAsync(report, cancellationToken);

                await _emailService.SendEmailAsync(recipient.Email!, subject, body, report.SchoolId);
            }

            // Mark report as sent
            report.IsSent = true;
            report.SentAt = DateTime.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        public async Task<List<WorkCompletionReport>> GetReportsForQACAsync(Guid schoolId, DateTime? fromDate = null, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var query = db.Set<WorkCompletionReport>()
                .AsNoTracking()
                .Include(r => r.Details)
                .Where(r => r.SchoolId == schoolId && r.IsSent);

            if (fromDate.HasValue)
            {
                query = query.Where(r => r.ReportDate >= fromDate.Value);
            }

            return await query.OrderByDescending(r => r.ReportDate).ToListAsync(cancellationToken);
        }

        private async Task<string> GenerateReportEmailBodyAsync(WorkCompletionReport report, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var plan = await db.Set<TeachingPlan>()
                .Include(p => p.Weeks)
                .FirstOrDefaultAsync(p => p.Id == report.TeachingPlanId, cancellationToken);

            var body = $@"
<h2>Work Completion Report</h2>
<p><strong>Report Date:</strong> {report.ReportDate:yyyy-MM-dd}</p>
<p><strong>Period:</strong> {report.PeriodStartDate:yyyy-MM-dd} to {report.PeriodEndDate:yyyy-MM-dd}</p>

<h3>Summary</h3>
<ul>
    <li>Total Periods: {report.TotalPeriods}</li>
    <li>Planned Periods: {report.PlannedPeriods}</li>
    <li>Completed Periods: {report.CompletedPeriods}</li>
    <li>Average % Planned: {report.AveragePercentagePlanned:F1}%</li>
    <li>Average % Completed: {report.AveragePercentageCompleted:F1}%</li>
    <li>Periods Behind Schedule: {report.PeriodsBehindSchedule}</li>
</ul>

<h3>Details</h3>
<table border=""1"" cellpadding=""5"" cellspacing=""0"">
    <tr>
        <th>Week</th>
        <th>Period</th>
        <th>Topic</th>
        <th>Date Planned</th>
        <th>% Planned</th>
        <th>Date Completed</th>
        <th>% Completed</th>
        <th>Status</th>
    </tr>
";

            foreach (var detail in report.Details)
            {
                var status = detail.IsLate ? $"Late ({detail.DaysBehind} days)" : (detail.IsOnSchedule ? "On Schedule" : "Pending");
                var statusColor = detail.IsLate ? "red" : (detail.IsOnSchedule ? "green" : "orange");

                body += $@"
    <tr>
        <td>{detail.WeekNumber}</td>
        <td>{detail.PeriodNumber}</td>
        <td>{detail.Topic} {detail.SubTopic}</td>
        <td>{detail.DatePlanned:yyyy-MM-dd}</td>
        <td>{detail.PercentagePlanned:F1}%</td>
        <td>{detail.DateCompleted:yyyy-MM-dd}</td>
        <td>{detail.PercentageCompleted:F1}%</td>
        <td style=""color: {statusColor}"">{status}</td>
    </tr>
";
            }

            body += @"
</table>
";

            return body;
        }
    }
}