using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lisa.Services.AcademicPlanning.DTOs;

namespace Lisa.Services.AcademicPlanning
{
    public interface IAcademicPlanningService
    {
        Task<AcademicPlanDto?> GetPlanAsync(
            Guid schoolId, 
            Guid schoolGradeId, 
            int subjectId, 
            Guid teacherId, 
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
        
        Task SubmitForReviewAsync(Guid planId, Guid userId, CancellationToken cancellationToken = default);

    }
}
