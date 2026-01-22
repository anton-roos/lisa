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
    public class SubjectGradePeriodService : ISubjectGradePeriodService
    {
        private readonly IDbContextFactory<LisaDbContext> _dbFactory;

        public SubjectGradePeriodService(IDbContextFactory<LisaDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<SubjectGradePeriod?> GetAsync(Guid schoolId, int subjectId, Guid schoolGradeId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<SubjectGradePeriod>()
                .AsNoTracking()
                .Include(p => p.Subject)
                .Include(p => p.SchoolGrade)
                    .ThenInclude(sg => sg!.SystemGrade)
                .Include(p => p.School)
                .FirstOrDefaultAsync(p => 
                    p.SchoolId == schoolId && 
                    p.SubjectId == subjectId && 
                    p.SchoolGradeId == schoolGradeId, cancellationToken);
        }

        public async Task<SubjectGradePeriod?> GetGlobalAsync(int subjectId, Guid schoolGradeId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<SubjectGradePeriod>()
                .AsNoTracking()
                .Include(p => p.Subject)
                .Include(p => p.SchoolGrade)
                    .ThenInclude(sg => sg!.SystemGrade)
                .FirstOrDefaultAsync(p => 
                    p.SchoolId == null && 
                    p.SubjectId == subjectId && 
                    p.SchoolGradeId == schoolGradeId, cancellationToken);
        }

        public async Task<List<SubjectGradePeriod>> GetBySchoolAsync(Guid schoolId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<SubjectGradePeriod>()
                .AsNoTracking()
                .Include(p => p.Subject)
                .Include(p => p.SchoolGrade)
                    .ThenInclude(sg => sg!.SystemGrade)
                .Include(p => p.School)
                .Where(p => p.SchoolId == schoolId)
                .OrderBy(p => p.Subject!.Name)
                .ThenBy(p => p.SchoolGrade!.SystemGrade!.SequenceNumber)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SubjectGradePeriod>> GetBySchoolGradeAsync(Guid schoolGradeId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<SubjectGradePeriod>()
                .AsNoTracking()
                .Include(p => p.Subject)
                .Include(p => p.SchoolGrade)
                    .ThenInclude(sg => sg!.SystemGrade)
                .Include(p => p.School)
                .Where(p => p.SchoolGradeId == schoolGradeId && (p.SchoolId == null || p.SchoolId == p.SchoolGrade!.SchoolId))
                .OrderBy(p => p.Subject!.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<SubjectGradePeriod>> GetBySubjectAsync(int subjectId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<SubjectGradePeriod>()
                .AsNoTracking()
                .Include(p => p.Subject)
                .Include(p => p.SchoolGrade)
                    .ThenInclude(sg => sg!.SystemGrade)
                .Include(p => p.School)
                .Where(p => p.SubjectId == subjectId)
                .OrderBy(p => p.SchoolGrade!.SystemGrade!.SequenceNumber)
                .ThenBy(p => p.School!.LongName)
                .ToListAsync(cancellationToken);
        }

        public async Task<SubjectGradePeriod> CreateOrUpdateAsync(SubjectGradePeriod period, Guid userId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.Set<SubjectGradePeriod>()
                .FirstOrDefaultAsync(p =>
                    p.SubjectId == period.SubjectId &&
                    p.SchoolGradeId == period.SchoolGradeId &&
                    ((period.SchoolId == null && p.SchoolId == null) || (period.SchoolId != null && p.SchoolId == period.SchoolId)),
                    cancellationToken);

            if (existing == null)
            {
                period.Id = Guid.NewGuid();
                period.CreatedBy = userId;
                period.CreatedAt = DateTime.UtcNow;
                period.UpdatedAt = DateTime.UtcNow;
                await db.Set<SubjectGradePeriod>().AddAsync(period, cancellationToken);
            }
            else
            {
                existing.PeriodsPerWeek = period.PeriodsPerWeek;
                existing.UpdatedBy = userId;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync(cancellationToken);
            return existing ?? period;
        }

        public async Task<bool> DeleteAsync(Guid periodId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var period = await db.Set<SubjectGradePeriod>().FindAsync(new object[] { periodId }, cancellationToken);
            if (period == null) return false;

            db.Set<SubjectGradePeriod>().Remove(period);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<Dictionary<string, Dictionary<string, int>>> GetSummaryBySchoolAsync(CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var periods = await db.Set<SubjectGradePeriod>()
                .AsNoTracking()
                .Include(p => p.Subject)
                .Include(p => p.SchoolGrade)
                    .ThenInclude(sg => sg!.SystemGrade)
                .Include(p => p.School)
                .ToListAsync(cancellationToken);

            var summary = new Dictionary<string, Dictionary<string, int>>();

            foreach (var period in periods)
            {
                var schoolName = period.School?.LongName ?? "Global";
                var gradeSubjectKey = $"{period.SchoolGrade?.SystemGrade?.Name} - {period.Subject?.Name}";

                if (!summary.ContainsKey(schoolName))
                {
                    summary[schoolName] = new Dictionary<string, int>();
                }

                summary[schoolName][gradeSubjectKey] = period.PeriodsPerWeek;
            }

            return summary;
        }
    }
}