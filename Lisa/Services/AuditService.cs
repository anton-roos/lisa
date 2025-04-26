using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services
{
    public class AuditService
    {
        private readonly IDbContextFactory<LisaDbContext> _dbContextFactory;

        public AuditService(IDbContextFactory<LisaDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task LogActivityAsync(string activityType, string description, Guid? entityId = null, Guid? userId = null)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();
            var auditLog = new AuditLog
            {
                ActivityType = activityType,
                Description = description,
                EntityId = entityId,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            await context.AuditLogs.AddAsync(auditLog);
            await context.SaveChangesAsync();
        }
    }
}