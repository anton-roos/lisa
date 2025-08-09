using Lisa.Data;
using Lisa.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lisa.Repositories;

public interface IEventLogRepository
{
    Task LogEventAsync(string eventType, string eventData);
}

public class EventLogRepository(IDbContextFactory<LisaDbContext> dbContextFactory, ILogger<EventLogRepository> logger)
    : IEventLogRepository
{
    /// <summary>
    /// Logs an event asynchronously with error handling.
    /// </summary>
    public async Task LogEventAsync(string eventType, string eventData)
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();

            var eventLog = new EventLog
            {
                EventType = eventType,
                EventData = eventData,
                CreatedAt = DateTime.UtcNow
            };

            context.EventLogs.Add(eventLog);
            await context.SaveChangesAsync();

            logger.LogInformation("Event logged: {EventType} at {Timestamp}", eventType, eventLog.CreatedAt);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error logging event: {EventType}", eventType);
        }
    }
}
