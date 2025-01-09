using System.Collections.Concurrent;
using Lisa.Interfaces;

namespace Lisa.Services;

public interface IUiEventService
{
    Guid Subscribe(string eventName, IEventSubscriber subscriber);
    void Unsubscribe(string eventName, Guid subscriberId);
    Task UnsubscribeAsync(string eventName, Guid subscriberId);
    Task PublishAsync(string eventName, object? payload = null);
}

public class UiEventService : IUiEventService, IDisposable
{
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, WeakReference<IEventSubscriber>>> _subscribers = new();
    private readonly SynchronizationContext? _syncContext;

    public UiEventService()
    {
        _syncContext = SynchronizationContext.Current;
    }

    public Guid Subscribe(string eventName, IEventSubscriber subscriber)
    {
        var subscriberId = Guid.NewGuid();
        var subscribers = _subscribers.GetOrAdd(eventName, _ => new ConcurrentDictionary<Guid, WeakReference<IEventSubscriber>>());
        subscribers[subscriberId] = new WeakReference<IEventSubscriber>(subscriber);
        
        CleanupDeadReferences(subscribers);

        return subscriberId;
    }

    public void Unsubscribe(string eventName, Guid subscriberId)
    {
        if (_subscribers.TryGetValue(eventName, out var subscribers))
        {
            subscribers.TryRemove(subscriberId, out _);
        }
    }

    public Task UnsubscribeAsync(string eventName, Guid subscriberId)
    {
        Unsubscribe(eventName, subscriberId);
        return Task.CompletedTask;
    }

    public async Task PublishAsync(string eventName, object? payload = null)
    {
        if (!_subscribers.TryGetValue(eventName, out var subscribers))
        {
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
                var taskCompletionSource = new TaskCompletionSource();
                _syncContext.Post(async _ =>
                {
                    try
                    {
                        await subscriber.HandleEventAsync(eventName, payload);
                        taskCompletionSource.SetResult();
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.SetException(ex);
                    }
                }, null);
                await taskCompletionSource.Task;
            }
            else
            {
                await subscriber.HandleEventAsync(eventName, payload);
            }
        }
        catch
        {
            // Log or handle exception as needed
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
        _subscribers.Clear();
        GC.SuppressFinalize(this);
    }
}