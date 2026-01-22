using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lisa.Services.AcademicPlanning.DTOs;
using Lisa.Models.AcademicPlanning;

namespace Lisa.Services.AcademicPlanning
{
    public interface IAcademicPlanningService
    {
        Task<AcademicPlanDto?> GetPlanAsync(
            Guid schoolId, 
            Guid schoolGradeId, 
            int subjectId, 
            Guid teacherId,
            Guid academicYearId,
            int term,
            CancellationToken cancellationToken = default);

        Task<List<AcademicPlanDto>> GetPlansBySchoolAsync(
            Guid schoolId,
            CancellationToken cancellationToken = default);

        Task<Guid> CreatePlanAsync(
            AcademicPlanDto dto,
            CancellationToken cancellationToken = default);

        Task<bool> UpdatePlanAsync(
            AcademicPlanDto dto,
            CancellationToken cancellationToken = default);

        Task<bool> DeletePlanAsync(
            Guid planId,
            CancellationToken cancellationToken = default);
        
        Task SubmitForReviewAsync(Guid planId, Guid userId, bool isSystemAdministrator = false, CancellationToken cancellationToken = default);
        Task<TeachingPlan?> GetTeachingPlanByIdAsync(Guid planId, CancellationToken cancellationToken = default);
        Task ApprovePlanAsync(Guid planId, Guid approverUserId, bool isSystemAdministrator = false, CancellationToken cancellationToken = default);
        Task RejectPlanAsync(Guid planId, Guid approverUserId, string reason, bool isSystemAdministrator = false, CancellationToken cancellationToken = default);
        Task<List<AcademicPlanHistoryDto>> GetPlanHistoryAsync(Guid planId, CancellationToken cancellationToken = default);
        Task<bool> SavePlanPeriodsAsync(Guid planId, List<AcademicPlanPeriod> periods, Guid userId, CancellationToken cancellationToken = default);
        Task<List<AcademicPlanDisplayDto>> GetPlansForDisplayAsync(Guid? schoolId, Guid? teacherId, CancellationToken cancellationToken = default);
    }
}
