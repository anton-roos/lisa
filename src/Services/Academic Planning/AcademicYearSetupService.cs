using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Lisa.Data;
using Lisa.Models.AcademicPlanning;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services.AcademicPlanning
{
    public class AcademicYearSetupService : IAcademicYearSetupService
    {
        private readonly IDbContextFactory<LisaDbContext> _dbFactory;

        public AcademicYearSetupService(IDbContextFactory<LisaDbContext> dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        public async Task<AcademicYearSetup?> GetBySchoolAndYearAsync(Guid schoolId, Guid academicYearId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<AcademicYearSetup>()
                .AsNoTracking()
                .Include(s => s.Terms)
                    .ThenInclude(t => t.Weeks)
                .Include(s => s.Holidays)
                .Include(s => s.AdministrativeDays)
                .Include(s => s.ExamDates)
                .FirstOrDefaultAsync(s => s.SchoolId == schoolId && s.AcademicYearId == academicYearId, cancellationToken);
        }

        public async Task<AcademicYearSetup> CreateOrUpdateAsync(AcademicYearSetup setup, Guid userId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.Set<AcademicYearSetup>()
                .Include(s => s.Terms)
                    .ThenInclude(t => t.Weeks)
                .Include(s => s.Holidays)
                .Include(s => s.AdministrativeDays)
                .Include(s => s.ExamDates)
                .FirstOrDefaultAsync(s => s.Id == setup.Id, cancellationToken);

            if (existing == null)
            {
                setup.Id = Guid.NewGuid();
                setup.CreatedBy = userId;
                setup.CreatedAt = DateTime.UtcNow;
                setup.UpdatedAt = DateTime.UtcNow;
                await db.Set<AcademicYearSetup>().AddAsync(setup, cancellationToken);
            }
            else
            {
                existing.UpdatedBy = userId;
                existing.UpdatedAt = DateTime.UtcNow;
                // Update relationships will be handled by individual methods
            }

            await db.SaveChangesAsync(cancellationToken);
            return setup;
        }

        public async Task<bool> DeleteAsync(Guid setupId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var setup = await db.Set<AcademicYearSetup>().FindAsync(new object[] { setupId }, cancellationToken);
            if (setup == null) return false;

            db.Set<AcademicYearSetup>().Remove(setup);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<AcademicTerm> AddTermAsync(Guid setupId, AcademicTerm term, CancellationToken cancellationToken = default)
        {
            term.Id = Guid.NewGuid();
            term.AcademicYearSetupId = setupId;
            term.CreatedAt = DateTime.UtcNow;
            term.UpdatedAt = DateTime.UtcNow;
            
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.Set<AcademicTerm>().AddAsync(term, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return term;
        }

        public async Task<bool> UpdateTermAsync(AcademicTerm term, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.Set<AcademicTerm>().FindAsync(new object[] { term.Id }, cancellationToken);
            if (existing == null) return false;

            existing.TermNumber = term.TermNumber;
            existing.StartDate = term.StartDate;
            existing.EndDate = term.EndDate;
            existing.ApplicablePhases = term.ApplicablePhases;
            existing.IsGrade12Specific = term.IsGrade12Specific;
            existing.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteTermAsync(Guid termId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var term = await db.Set<AcademicTerm>().FindAsync(new object[] { termId }, cancellationToken);
            if (term == null) return false;

            db.Set<AcademicTerm>().Remove(term);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<TermWeek> AddWeekAsync(Guid termId, TermWeek week, CancellationToken cancellationToken = default)
        {
            week.Id = Guid.NewGuid();
            week.AcademicTermId = termId;
            week.CreatedAt = DateTime.UtcNow;
            week.UpdatedAt = DateTime.UtcNow;
            
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.Set<TermWeek>().AddAsync(week, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return week;
        }

        public async Task<bool> UpdateWeekAsync(TermWeek week, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.Set<TermWeek>().FindAsync(new object[] { week.Id }, cancellationToken);
            if (existing == null) return false;

            existing.WeekNumber = week.WeekNumber;
            existing.StartDate = week.StartDate;
            existing.EndDate = week.EndDate;
            existing.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteWeekAsync(Guid weekId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var week = await db.Set<TermWeek>().FindAsync(new object[] { weekId }, cancellationToken);
            if (week == null) return false;

            db.Set<TermWeek>().Remove(week);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<Holiday> AddHolidayAsync(Guid setupId, Holiday holiday, CancellationToken cancellationToken = default)
        {
            holiday.Id = Guid.NewGuid();
            holiday.AcademicYearSetupId = setupId;
            holiday.CreatedAt = DateTime.UtcNow;
            holiday.UpdatedAt = DateTime.UtcNow;
            
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.Set<Holiday>().AddAsync(holiday, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return holiday;
        }

        public async Task<bool> UpdateHolidayAsync(Holiday holiday, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.Set<Holiday>().FindAsync(new object[] { holiday.Id }, cancellationToken);
            if (existing == null) return false;

            existing.Name = holiday.Name;
            existing.Date = holiday.Date;
            existing.Description = holiday.Description;
            existing.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteHolidayAsync(Guid holidayId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var holiday = await db.Set<Holiday>().FindAsync(new object[] { holidayId }, cancellationToken);
            if (holiday == null) return false;

            db.Set<Holiday>().Remove(holiday);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<AdministrativeDay> AddAdministrativeDayAsync(Guid setupId, AdministrativeDay adminDay, CancellationToken cancellationToken = default)
        {
            adminDay.Id = Guid.NewGuid();
            adminDay.AcademicYearSetupId = setupId;
            adminDay.CreatedAt = DateTime.UtcNow;
            adminDay.UpdatedAt = DateTime.UtcNow;
            
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.Set<AdministrativeDay>().AddAsync(adminDay, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return adminDay;
        }

        public async Task<bool> UpdateAdministrativeDayAsync(AdministrativeDay adminDay, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.Set<AdministrativeDay>().FindAsync(new object[] { adminDay.Id }, cancellationToken);
            if (existing == null) return false;

            existing.Name = adminDay.Name;
            existing.Date = adminDay.Date;
            existing.Description = adminDay.Description;
            existing.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteAdministrativeDayAsync(Guid adminDayId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var adminDay = await db.Set<AdministrativeDay>().FindAsync(new object[] { adminDayId }, cancellationToken);
            if (adminDay == null) return false;

            db.Set<AdministrativeDay>().Remove(adminDay);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<ExamDate> AddExamDateAsync(Guid setupId, ExamDate examDate, CancellationToken cancellationToken = default)
        {
            examDate.Id = Guid.NewGuid();
            examDate.AcademicYearSetupId = setupId;
            examDate.CreatedAt = DateTime.UtcNow;
            examDate.UpdatedAt = DateTime.UtcNow;
            
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            await db.Set<ExamDate>().AddAsync(examDate, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
            return examDate;
        }

        public async Task<bool> UpdateExamDateAsync(ExamDate examDate, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var existing = await db.Set<ExamDate>().FindAsync(new object[] { examDate.Id }, cancellationToken);
            if (existing == null) return false;

            existing.Name = examDate.Name;
            existing.Date = examDate.Date;
            existing.ApplicablePhases = examDate.ApplicablePhases;
            existing.IsGrade12Specific = examDate.IsGrade12Specific;
            existing.Description = examDate.Description;
            existing.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteExamDateAsync(Guid examDateId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var examDate = await db.Set<ExamDate>().FindAsync(new object[] { examDateId }, cancellationToken);
            if (examDate == null) return false;

            db.Set<ExamDate>().Remove(examDate);
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<List<AcademicTerm>> GetTermsForYearAsync(Guid setupId, string? phaseFilter = null, bool includeGrade12 = false, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var query = db.Set<AcademicTerm>()
                .AsNoTracking()
                .Include(t => t.Weeks)
                .Where(t => t.AcademicYearSetupId == setupId);

            if (!string.IsNullOrEmpty(phaseFilter))
            {
                query = query.Where(t => t.ApplicablePhases != null && t.ApplicablePhases.Contains(phaseFilter));
            }

            if (includeGrade12)
            {
                query = query.Where(t => t.IsGrade12Specific);
            }

            return await query.OrderBy(t => t.TermNumber).ToListAsync(cancellationToken);
        }

        public async Task<List<Holiday>> GetHolidaysForYearAsync(Guid setupId, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            return await db.Set<Holiday>()
                .AsNoTracking()
                .Where(h => h.AcademicYearSetupId == setupId)
                .OrderBy(h => h.Date)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ExamDate>> GetExamDatesForYearAsync(Guid setupId, string? phaseFilter = null, bool includeGrade12 = false, CancellationToken cancellationToken = default)
        {
            await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
            var query = db.Set<ExamDate>()
                .AsNoTracking()
                .Where(e => e.AcademicYearSetupId == setupId);

            if (!string.IsNullOrEmpty(phaseFilter))
            {
                query = query.Where(e => e.ApplicablePhases != null && e.ApplicablePhases.Contains(phaseFilter));
            }

            if (includeGrade12)
            {
                query = query.Where(e => e.IsGrade12Specific);
            }

            return await query.OrderBy(e => e.Date).ToListAsync(cancellationToken);
        }
    }
}