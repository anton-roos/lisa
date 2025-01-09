using Lisa.Interfaces;
using Lisa.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;

namespace Lisa.Components;

public abstract class EventAwareComponentBase : ComponentBase, IEventSubscriber, IDisposable, IAsyncDisposable
{
    [Inject]
    protected IUiEventService IUiEventService { get; set; } = default!;

    private readonly ConcurrentDictionary<string, Guid> _subscriptionIds = new();
    private bool _disposed;

    protected void SubscribeToEvent(string eventName)
    {
        if (!_subscriptionIds.ContainsKey(eventName))
        {
            var id = IUiEventService.Subscribe(eventName, this);
            _subscriptionIds.TryAdd(eventName, id);
        }
    }

    // Explicit interface implementation for event handling
    Task IEventSubscriber.HandleEventAsync(string eventName, object? payload)
    {
        return HandleEventAsync(eventName, payload);
    }

    // Virtual method that derived classes can override
    protected virtual async Task HandleEventAsync(string eventName, object? payload)
    {
        if (!_disposed)
        {
            try
            {
                await InvokeAsync(StateHasChanged);
            }
            catch (ObjectDisposedException)
            {
                // Component has been disposed; safe to ignore
            }
        }
    }

    // Cleanup logic for synchronous disposal
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    // Asynchronous disposal
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    // Core asynchronous cleanup logic
    protected virtual async ValueTask DisposeAsyncCore()
    {
        foreach (var subscription in _subscriptionIds)
        {
            await IUiEventService.UnsubscribeAsync(subscription.Key, subscription.Value);
        }
        _subscriptionIds.Clear();
    }

    // Core synchronous cleanup logic
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                foreach (var subscription in _subscriptionIds)
                {
                    IUiEventService.Unsubscribe(subscription.Key, subscription.Value);
                }
                _subscriptionIds.Clear();
            }

            _disposed = true;
        }
    }

    // Optional: Provide a hook for derived classes to add custom disposal logic
    protected virtual void OnDisposing() { }
}
