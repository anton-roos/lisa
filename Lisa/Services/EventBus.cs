using System.Collections.Concurrent;
using Lisa.Repositories;
using Newtonsoft.Json;

namespace Lisa.Services;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : class;
    void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : class;
}

public class EventBus(IServiceProvider serviceProvider) : IEventBus
{
    private readonly ConcurrentDictionary<Type, List<Func<object, Task>>> _handlers = new();
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : class
    {
        var eventType = typeof(TEvent).Name;

        var settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        var eventData = JsonConvert.SerializeObject(@event, settings);

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var eventLogRepository = scope.ServiceProvider.GetRequiredService<IEventLogRepository>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<EventBus>>();

            await eventLogRepository.LogEventAsync(eventType, eventData);
            logger.LogInformation("Event published: {EventType}", eventType);

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
            var logger = _serviceProvider.GetRequiredService<ILogger<EventBus>>();
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