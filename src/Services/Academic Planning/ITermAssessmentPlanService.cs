using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lisa.Models.AcademicPlanning;

namespace Lisa.Services.AcademicPlanning
{
    public interface ITermAssessmentPlanService
    {
        Task<TermAssessmentPlan?> GetAsync(Guid schoolId, Guid schoolGradeId, Guid academicYearId, int term, CancellationToken cancellationToken = default);
        Task<TermAssessmentPlan?> GetByIdAsync(Guid planId, CancellationToken cancellationToken = default);
        Task<List<TermAssessmentPlan>> GetBySchoolAsync(Guid schoolId, CancellationToken cancellationToken = default);
        Task<TermAssessmentPlan> CreateOrUpdateAsync(TermAssessmentPlan plan, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid planId, CancellationToken cancellationToken = default);
        
        // Assessment scheduling
        Task<ScheduledAssessment> ScheduleAssessmentAsync(Guid planId, ScheduledAssessment assessment, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> UpdateAssessmentAsync(ScheduledAssessment assessment, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> DeleteAssessmentAsync(Guid assessmentId, CancellationToken cancellationToken = default);
        Task<bool> CanScheduleAssessmentAsync(Guid planId, DateTime date, Guid? excludeAssessmentId = null, CancellationToken cancellationToken = default);
        
        // Assessment status
        Task<bool> MarkAssessmentCompletedAsync(Guid assessmentId, Guid resultSetId, CancellationToken cancellationToken = default);
        Task<bool> UpdateAssessmentMarksStatusAsync(Guid assessmentId, bool marksCaptured, bool isLate, CancellationToken cancellationToken = default);
        
        // Views
        Task<List<ScheduledAssessment>> GetAssessmentsByWeekAsync(Guid planId, int weekNumber, CancellationToken cancellationToken = default);
        Task<List<ScheduledAssessment>> GetAssessmentsByDateAsync(Guid planId, DateTime date, CancellationToken cancellationToken = default);
        Task<Dictionary<DateTime, List<ScheduledAssessment>>> GetAssessmentCalendarAsync(Guid planId, CancellationToken cancellationToken = default);
    }
}