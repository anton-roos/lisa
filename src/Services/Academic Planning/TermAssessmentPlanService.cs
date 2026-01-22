using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lisa.Data;
using Lisa.Models.AcademicPlanning;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services.AcademicPlanning
{
    public class TermAssessmentPlanService : ITermAssessmentPlanService
    {
        private readonly IDbContextFactory<LisaDbContext> _dbFactory;

        public TermAssessmentPlanService(IDbContextFactory<LisaDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<TermAssessmentPlan?> GetAsync(Guid schoolId, Guid schoolGradeId, Guid academicYearId, int term, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<TermAssessmentPlan>()
                .AsNoTracking()
                .Include(p => p.ScheduledAssessments)
                    .ThenInclude(a => a.Subject)
                .Include(p => p.ScheduledAssessments)
                    .ThenInclude(a => a.ResultSet)
                .FirstOrDefaultAsync(p =>
                    p.SchoolId == schoolId &&
                    p.SchoolGradeId == schoolGradeId &&
                    p.AcademicYearId == academicYearId &&
                    p.Term == term,
                    cancellationToken);
        }

        public async Task<TermAssessmentPlan?> GetByIdAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<TermAssessmentPlan>()
                .AsNoTracking()
                .Include(p => p.ScheduledAssessments)
                    .ThenInclude(a => a.Subject)
                .Include(p => p.ScheduledAssessments)
                    .ThenInclude(a => a.ResultSet)
                .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);
        }

        public async Task<List<TermAssessmentPlan>> GetBySchoolAsync(Guid schoolId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<TermAssessmentPlan>()
                .AsNoTracking()
                .Include(p => p.ScheduledAssessments)
                    .ThenInclude(a => a.Subject)
                .OrderBy(p => p.AcademicYearId)
                .ThenBy(p => p.Term)
                .ToListAsync(cancellationToken);
        }

        public async Task<TermAssessmentPlan> CreateOrUpdateAsync(TermAssessmentPlan plan, Guid userId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.Set<TermAssessmentPlan>()
                .Include(p => p.ScheduledAssessments)
                .FirstOrDefaultAsync(p =>
                    p.SchoolId == plan.SchoolId &&
                    p.SchoolGradeId == plan.SchoolGradeId &&
                    p.AcademicYearId == plan.AcademicYearId &&
                    p.Term == plan.Term,
                    cancellationToken);

            if (existing == null)
            {
                plan.Id = Guid.NewGuid();
                plan.CreatedBy = userId;
                plan.CreatedAt = DateTime.UtcNow;
                plan.UpdatedAt = DateTime.UtcNow;
                await db.Set<TermAssessmentPlan>().AddAsync(plan, cancellationToken);
            }
            else
            {
                existing.UpdatedBy = userId;
                existing.UpdatedAt = DateTime.UtcNow;
                // Assessments are managed separately
            }

            await db.SaveChangesAsync(cancellationToken);
            return existing ?? plan;
        }

        public async Task<bool> DeleteAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var plan = await db.Set<TermAssessmentPlan>()
                .Include(p => p.ScheduledAssessments)
                .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);
            
            if (plan == null) return false;

            db.Set<TermAssessmentPlan>().Remove(plan);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<ScheduledAssessment> ScheduleAssessmentAsync(Guid planId, ScheduledAssessment assessment, Guid userId, CancellationToken cancellationToken = default)
        {
            // Check if another assessment is already scheduled for this date
            var canSchedule = await CanScheduleAssessmentAsync(planId, assessment.ScheduledDate, null, cancellationToken);
            if (!canSchedule)
            {
                throw new InvalidOperationException($"Another assessment is already scheduled for {assessment.ScheduledDate:yyyy-MM-dd}");
            }

            assessment.Id = Guid.NewGuid();
            assessment.TermAssessmentPlanId = planId;
            assessment.IsCompleted = true; // Strike through when scheduled
            assessment.CreatedBy = userId;
            assessment.CreatedAt = DateTime.UtcNow;
            assessment.UpdatedAt = DateTime.UtcNow;

            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.Set<ScheduledAssessment>().AddAsync(assessment, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return assessment;
        }

        public async Task<bool> UpdateAssessmentAsync(ScheduledAssessment assessment, Guid userId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.Set<ScheduledAssessment>().FindAsync(new object[] { assessment.Id }, cancellationToken);
            if (existing == null) return false;

            // Check if date changed and conflicts with another assessment
            if (existing.ScheduledDate != assessment.ScheduledDate)
            {
                var canSchedule = await CanScheduleAssessmentAsync(existing.TermAssessmentPlanId, assessment.ScheduledDate, assessment.Id, cancellationToken);
                if (!canSchedule)
                {
                    throw new InvalidOperationException($"Another assessment is already scheduled for {assessment.ScheduledDate:yyyy-MM-dd}");
                }
            }

            existing.AssessmentName = assessment.AssessmentName;
            existing.SubjectId = assessment.SubjectId;
            existing.AssessmentType = assessment.AssessmentType;
            existing.ScheduledDate = assessment.ScheduledDate;
            existing.WeekNumber = assessment.WeekNumber;
            existing.UpdatedBy = userId;
            existing.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAssessmentAsync(Guid assessmentId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var assessment = await db.Set<ScheduledAssessment>().FindAsync(new object[] { assessmentId }, cancellationToken);
            if (assessment == null) return false;

            db.Set<ScheduledAssessment>().Remove(assessment);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> CanScheduleAssessmentAsync(Guid planId, DateTime date, Guid? excludeAssessmentId = null, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var query = db.Set<ScheduledAssessment>()
                .Where(a => a.TermAssessmentPlanId == planId && a.ScheduledDate.Date == date.Date);

            if (excludeAssessmentId.HasValue)
            {
                query = query.Where(a => a.Id != excludeAssessmentId.Value);
            }

            return !await query.AnyAsync(cancellationToken);
        }

        public async Task<bool> MarkAssessmentCompletedAsync(Guid assessmentId, Guid resultSetId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var assessment = await db.Set<ScheduledAssessment>().FindAsync(new object[] { assessmentId }, cancellationToken);
            if (assessment == null) return false;

            assessment.ResultSetId = resultSetId;
            assessment.MarksCaptured = true;
            assessment.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> UpdateAssessmentMarksStatusAsync(Guid assessmentId, bool marksCaptured, bool isLate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var assessment = await db.Set<ScheduledAssessment>().FindAsync(new object[] { assessmentId }, cancellationToken);
            if (assessment == null) return false;

            assessment.MarksCaptured = marksCaptured;
            assessment.IsMarksLate = isLate;
            assessment.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<ScheduledAssessment>> GetAssessmentsByWeekAsync(Guid planId, int weekNumber, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<ScheduledAssessment>()
                .AsNoTracking()
                .Include(a => a.Subject)
                .Where(a => a.TermAssessmentPlanId == planId && a.WeekNumber == weekNumber)
                .OrderBy(a => a.ScheduledDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ScheduledAssessment>> GetAssessmentsByDateAsync(Guid planId, DateTime date, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<ScheduledAssessment>()
                .AsNoTracking()
                .Include(a => a.Subject)
                .Where(a => a.TermAssessmentPlanId == planId && a.ScheduledDate.Date == date.Date)
                .OrderBy(a => a.ScheduledDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<DateTime, List<ScheduledAssessment>>> GetAssessmentCalendarAsync(Guid planId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var assessments = await db.Set<ScheduledAssessment>()
                .AsNoTracking()
                .Include(a => a.Subject)
                .Where(a => a.TermAssessmentPlanId == planId)
                .OrderBy(a => a.ScheduledDate)
                .ToListAsync(cancellationToken);

            var calendar = new Dictionary<DateTime, List<ScheduledAssessment>>();

            foreach (var assessment in assessments)
            {
                var date = assessment.ScheduledDate.Date;
                if (!calendar.ContainsKey(date))
                {
                    calendar[date] = new List<ScheduledAssessment>();
                }
                calendar[date].Add(assessment);
            }

            return calendar;
        }
    }
}