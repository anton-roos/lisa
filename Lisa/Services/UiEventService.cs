using System.Collections.Concurrent;
using Lisa.Interfaces;

namespace Lisa.Services;

public class UiEventService(ILogger<UiEventService> logger) : IDisposable
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, WeakReference<IEventSubscriber>>> _subscribers = new();
    private readonly ILogger<UiEventService> _logger = logger;
    private readonly SynchronizationContext? _syncContext = SynchronizationContext.Current;

    /// <summary>
    /// Subscribes an event subscriber to a given event name.
    /// </summary>
    public Guid Subscribe(string eventName, IEventSubscriber subscriber)
    {
        var subscriberId = Guid.NewGuid();
        var subscribers = _subscribers.GetOrAdd(eventName, _ => new ConcurrentDictionary<Guid, WeakReference<IEventSubscriber>>());
        subscribers[subscriberId] = new WeakReference<IEventSubscriber>(subscriber);

        CleanupDeadReferences(subscribers);

        _logger.LogInformation("Subscriber {SubscriberId} subscribed to event {EventName}.", subscriberId, eventName);
        return subscriberId;
    }

    /// <summary>
    /// Unsubscribes a subscriber from an event.
    /// </summary>
    public void Unsubscribe(string eventName, Guid subscriberId)
    {
        if (_subscribers.TryGetValue(eventName, out var subscribers))
        {
            subscribers.TryRemove(subscriberId, out _);
            CleanupDeadReferences(subscribers);
            _logger.LogInformation("Subscriber {SubscriberId} unsubscribed from event {EventName}.", subscriberId, eventName);
        }
    }

    /// <summary>
    /// Asynchronous version of Unsubscribe.
    /// </summary>
    public Task UnsubscribeAsync(string eventName, Guid subscriberId)
    {
        Unsubscribe(eventName, subscriberId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Publishes an event to all subscribers.
    /// </summary>
    public async Task PublishAsync(string eventName, object? payload)
    {
        if (!_subscribers.TryGetValue(eventName, out var subscribers) || subscribers.IsEmpty)
        {
            _logger.LogWarning("No subscribers found for event {EventName}.", eventName);
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

    /// <summary>
    /// Safely invokes an event subscriber.
    /// </summary>
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
            _logger.LogError(ex, "Error invoking subscriber for event {EventName}.", eventName);
        }
    }

    /// <summary>
    /// Cleans up dead references from the subscriber dictionary.
    /// </summary>
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

    /// <summary>
    /// Disposes of the event service and cleans up all subscriptions.
    /// </summary>
    public void Dispose()
    {
        foreach (var eventSubscribers in _subscribers.Values)
        {
            eventSubscribers.Clear();
        }
        _subscribers.Clear();
        GC.SuppressFinalize(this);
        _logger.LogInformation("UiEventService disposed.");
    }
}
