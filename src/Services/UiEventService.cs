using Lisa.Interfaces;
using System.Collections.Concurrent;

namespace Lisa.Services;

public class UiEventService
(
    ILogger<UiEventService> logger
) : IDisposable
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, WeakReference<IEventSubscriber>>> _subscribers = new();
    private readonly SynchronizationContext? _syncContext = SynchronizationContext.Current;

    public Guid Subscribe(string eventName, IEventSubscriber subscriber)
    {
        var subscriberId = Guid.NewGuid();
        var subscribers = _subscribers.GetOrAdd(eventName, _ => new ConcurrentDictionary<Guid, WeakReference<IEventSubscriber>>());
        subscribers[subscriberId] = new WeakReference<IEventSubscriber>(subscriber);

        CleanupDeadReferences(subscribers);

        logger.LogInformation("Subscriber {SubscriberId} subscribed to event {EventName}.", subscriberId, eventName);
        return subscriberId;
    }

    public void Unsubscribe(string eventName, Guid subscriberId)
    {
        if (_subscribers.TryGetValue(eventName, out var subscribers))
        {
            subscribers.TryRemove(subscriberId, out _);
            CleanupDeadReferences(subscribers);
            logger.LogInformation("Subscriber {SubscriberId} unsubscribed from event {EventName}.", subscriberId, eventName);
        }
    }

    public Task UnsubscribeAsync(string eventName, Guid subscriberId)
    {
        Unsubscribe(eventName, subscriberId);
        return Task.CompletedTask;
    }

    public async Task PublishAsync(string eventName, object? payload)
    {
        if (!_subscribers.TryGetValue(eventName, out var subscribers) || subscribers.IsEmpty)
        {
            logger.LogWarning("No subscribers found for event {EventName}.", eventName);
            return;
        }

        var tasks = new List<Task>();

        foreach (var kvp in subscribers)
        {
            if (kvp.Value.TryGetTarget(out var subscriber))
            {
                tasks.Add(InvokeSubscriberAsync(subscriber, eventName, payload));
            }
            else
            {
                subscribers.TryRemove(kvp.Key, out _);
            }
        }

        await Task.WhenAll(tasks);
    }

    private async Task InvokeSubscriberAsync(IEventSubscriber subscriber, string eventName, object? payload)
    {
        try
        {
            if (_syncContext != null)
            {
                await Task.Run(async () => await subscriber.HandleEventAsync(eventName, payload));
            }
            else
            {
                await subscriber.HandleEventAsync(eventName, payload);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invoking subscriber for event {EventName}.", eventName);
        }
    }

    private static void CleanupDeadReferences(ConcurrentDictionary<Guid, WeakReference<IEventSubscriber>> subscribers)
    {
        var deadSubscribers = subscribers
            .Where(kvp => !kvp.Value.TryGetTarget(out _))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var id in deadSubscribers)
        {
            subscribers.TryRemove(id, out _);
        }
    }

    public void Dispose()
    {
        foreach (var eventSubscribers in _subscribers.Values)
        {
            eventSubscribers.Clear();
        }
        _subscribers.Clear();
        GC.SuppressFinalize(this);
        logger.LogInformation("UiEventService disposed.");
    }
}