using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lisa.Models.AcademicPlanning;

namespace Lisa.Services.AcademicPlanning
{
    public interface ISubjectGradePeriodService
    {
        Task<SubjectGradePeriod?> GetAsync(Guid schoolId, int subjectId, Guid schoolGradeId, CancellationToken cancellationToken = default);
        Task<SubjectGradePeriod?> GetGlobalAsync(int subjectId, Guid schoolGradeId, CancellationToken cancellationToken = default);
        Task<List<SubjectGradePeriod>> GetBySchoolAsync(Guid schoolId, CancellationToken cancellationToken = default);
        Task<List<SubjectGradePeriod>> GetBySchoolGradeAsync(Guid schoolGradeId, CancellationToken cancellationToken = default);
        Task<List<SubjectGradePeriod>> GetBySubjectAsync(int subjectId, CancellationToken cancellationToken = default);
        Task<SubjectGradePeriod> CreateOrUpdateAsync(SubjectGradePeriod period, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid periodId, CancellationToken cancellationToken = default);
        Task<Dictionary<string, Dictionary<string, int>>> GetSummaryBySchoolAsync(CancellationToken cancellationToken = default);
    }
}