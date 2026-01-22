using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lisa.Models.AcademicPlanning;
using Lisa.Models.Entities;

namespace Lisa.Services.AcademicPlanning
{
    public interface IWorkCompletionReportService
    {
        // Report generation
        Task<WorkCompletionReport> GenerateReportAsync(Guid teachingPlanId, DateTime periodStartDate, DateTime periodEndDate, CancellationToken cancellationToken = default);
        Task<List<WorkCompletionReport>> GenerateReportsForSchoolAsync(Guid schoolId, DateTime periodStartDate, DateTime periodEndDate, CancellationToken cancellationToken = default);
        
        // Report retrieval
        Task<WorkCompletionReport?> GetReportAsync(Guid reportId, CancellationToken cancellationToken = default);
        Task<List<WorkCompletionReport>> GetReportsByTeachingPlanAsync(Guid teachingPlanId, CancellationToken cancellationToken = default);
        Task<List<WorkCompletionReport>> GetReportsBySchoolAsync(Guid schoolId, CancellationToken cancellationToken = default);
        
        // Recipient management
        Task<WorkCompletionReportRecipient> AddRecipientAsync(Guid schoolId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> RemoveRecipientAsync(Guid recipientId, CancellationToken cancellationToken = default);
        Task<List<WorkCompletionReportRecipient>> GetRecipientsBySchoolAsync(Guid schoolId, CancellationToken cancellationToken = default);
        Task<List<User>> GetRecipientsForSchoolAsync(Guid schoolId, CancellationToken cancellationToken = default);
        
        // Email automation
        Task SendWeeklyReportsAsync(Guid schoolId, CancellationToken cancellationToken = default);
        Task SendReportAsync(WorkCompletionReport report, List<Guid> recipientUserIds, CancellationToken cancellationToken = default);
        
        // Integration with QAC Delivery Control
        Task<List<WorkCompletionReport>> GetReportsForQACAsync(Guid schoolId, DateTime? fromDate = null, CancellationToken cancellationToken = default);
    }
}