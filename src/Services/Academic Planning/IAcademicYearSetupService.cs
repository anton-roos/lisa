using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lisa.Models.AcademicPlanning;

namespace Lisa.Services.AcademicPlanning
{
    public interface IAcademicYearSetupService
    {
        Task<AcademicYearSetup?> GetBySchoolAndYearAsync(Guid schoolId, Guid academicYearId, CancellationToken cancellationToken = default);
        Task<AcademicYearSetup> CreateOrUpdateAsync(AcademicYearSetup setup, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid setupId, CancellationToken cancellationToken = default);
        
        // Term management
        Task<AcademicTerm> AddTermAsync(Guid setupId, AcademicTerm term, CancellationToken cancellationToken = default);
        Task<bool> UpdateTermAsync(AcademicTerm term, CancellationToken cancellationToken = default);
        Task<bool> DeleteTermAsync(Guid termId, CancellationToken cancellationToken = default);
        
        // Week management
        Task<TermWeek> AddWeekAsync(Guid termId, TermWeek week, CancellationToken cancellationToken = default);
        Task<bool> UpdateWeekAsync(TermWeek week, CancellationToken cancellationToken = default);
        Task<bool> DeleteWeekAsync(Guid weekId, CancellationToken cancellationToken = default);
        
        // Holiday management
        Task<Holiday> AddHolidayAsync(Guid setupId, Holiday holiday, CancellationToken cancellationToken = default);
        Task<bool> UpdateHolidayAsync(Holiday holiday, CancellationToken cancellationToken = default);
        Task<bool> DeleteHolidayAsync(Guid holidayId, CancellationToken cancellationToken = default);
        
        // Administrative day management
        Task<AdministrativeDay> AddAdministrativeDayAsync(Guid setupId, AdministrativeDay adminDay, CancellationToken cancellationToken = default);
        Task<bool> UpdateAdministrativeDayAsync(AdministrativeDay adminDay, CancellationToken cancellationToken = default);
        Task<bool> DeleteAdministrativeDayAsync(Guid adminDayId, CancellationToken cancellationToken = default);
        
        // Exam date management
        Task<ExamDate> AddExamDateAsync(Guid setupId, ExamDate examDate, CancellationToken cancellationToken = default);
        Task<bool> UpdateExamDateAsync(ExamDate examDate, CancellationToken cancellationToken = default);
        Task<bool> DeleteExamDateAsync(Guid examDateId, CancellationToken cancellationToken = default);
        
        // Summary views
        Task<List<AcademicTerm>> GetTermsForYearAsync(Guid setupId, string? phaseFilter = null, bool includeGrade12 = false, CancellationToken cancellationToken = default);
        Task<List<Holiday>> GetHolidaysForYearAsync(Guid setupId, CancellationToken cancellationToken = default);
        Task<List<ExamDate>> GetExamDatesForYearAsync(Guid setupId, string? phaseFilter = null, bool includeGrade12 = false, CancellationToken cancellationToken = default);
    }
}