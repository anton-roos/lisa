using System.Collections.Concurrent;
using Lisa.Repositories;
using Newtonsoft.Json;

namespace Lisa.Services;

public interface IEventBus
{
    void Publish<TEvent>(TEvent @event) where TEvent : class;
    void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class;
}

public class EventBus(IServiceProvider serviceProvider) : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Func<object, Task>>> _handlers = new();
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async void Publish<TEvent>(TEvent @event) where TEvent : class
    {
        var eventType = typeof(TEvent).Name;
        
        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        var eventData = JsonConvert.SerializeObject(@event, settings);

        try
        {
            // Resolve a scope for logging the event to the database
            using var scope = _serviceProvider.CreateScope();
            var eventLogRepository = scope.ServiceProvider.GetRequiredService<IEventLogRepository>();

            // Log the event to the database
            await eventLogRepository.LogEventAsync(eventType, eventData);

            // Fire all registered handlers for the event
            if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
            {
                foreach (var handler in handlers)
                {
                    Task.Run(() => handler(@event));
                }
            }
        }
        catch (Exception ex)
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<EventBus>>();
            logger.LogError(ex, "An error occurred while publishing event: {EventType}", eventType);
        }
    }

    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class
    {
        var eventType = typeof(TEvent);
        if (!_handlers.ContainsKey(eventType))
        {
            _handlers[eventType] = [];
        }

        _handlers[eventType].Add(e => handler((TEvent)e));
    }
}
