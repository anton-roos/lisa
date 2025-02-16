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
    private readonly IDbContextFactory<LisaDbContext> _dbContextFactory = dbContextFactory;
    private readonly ILogger<EventLogRepository> _logger = logger;

    /// <summary>
    /// Logs an event asynchronously with error handling.
    /// </summary>
    public async Task LogEventAsync(string eventType, string eventData)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var eventLog = new EventLog
            {
                EventType = eventType,
                EventData = eventData,
                CreatedAt = DateTime.UtcNow
            };

            context.EventLogs.Add(eventLog);
            await context.SaveChangesAsync();

            _logger.LogInformation("Event logged: {EventType} at {Timestamp}", eventType, eventLog.CreatedAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging event: {EventType}", eventType);
        }
    }

    /// <summary>
    /// Retrieves all event logs with pagination (default: 50 per page).
    /// </summary>
    public async Task<List<EventLog>> GetAllEventLogsAsync(int pageNumber = 1, int pageSize = 50)
    {
        try
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            return await context.EventLogs
                .AsNoTracking()
                .OrderByDescending(e => e.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving event logs.");
            return [];
        }
    }
}
