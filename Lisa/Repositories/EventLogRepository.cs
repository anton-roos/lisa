using Lisa.Data;
using Lisa.Models.Entities;

namespace Lisa.Repositories;

public interface IEventLogRepository
{
    Task LogEventAsync(string eventType, string eventData);
}

public class EventLogRepository(LisaDbContext context) : IEventLogRepository
{
    private readonly LisaDbContext _context = context;

    public async Task LogEventAsync(string eventType, string eventData)
    {
        var eventLog = new EventLog
        {
            EventType = eventType,
            EventData = eventData,
            CreatedAt = DateTime.UtcNow
        };

        _context.Set<EventLog>().Add(eventLog);
        await _context.SaveChangesAsync();
    }
}
