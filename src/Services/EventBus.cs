using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lisa.Services;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : class;
}

public class EventBus(IServiceProvider serviceProvider) : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Func<object, Task>>> _handlers = new();

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
    {
        var eventType = typeof(TEvent).Name;

        var options = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };

        var eventData = JsonSerializer.Serialize(@event, options);

        try
        {
            using var scope = serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<EventBus>>();

            if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
            {
                var tasks = handlers.Select(handler =>
                {
                    try
                    {
                        return handler(@event);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error handling event {EventType}", eventType);
                        return Task.CompletedTask;
                    }
                }).ToArray();

                await Task.WhenAll(tasks);
            }
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<EventBus>>();
            logger.LogError(ex, "An error occurred while publishing event: {EventType}", eventType);
        }
    }

    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class
    {
        var eventType = typeof(TEvent);
        _handlers
            .GetOrAdd(eventType, _ => [])
            .Add(e => handler((TEvent)e));
    }
}