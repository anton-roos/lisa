using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Services;

public class AuditService(IDbContextFactory<LisaDbContext> dbContextFactory)
{
    public async Task LogActivityAsync(string activityType, string description, Guid? entityId = null, Guid? userId = null)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
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