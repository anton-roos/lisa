using Lisa.Interfaces;
using Lisa.Services;
using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;

namespace Lisa.Pages;

public abstract class EventAwareComponentBase : ComponentBase, IEventSubscriber, IDisposable, IAsyncDisposable
{
    [Inject]
    protected UiEventService UiEventService { get; set; } = null!;

    private readonly ConcurrentDictionary<string, Guid> _subscriptionIds = new();
    private bool _disposed;

    protected void SubscribeToEvent(string eventName)
    {
        if (_subscriptionIds.ContainsKey(eventName))
        {
            return;
        }

        var id = UiEventService.Subscribe(eventName, this);
        _subscriptionIds.TryAdd(eventName, id);
    }

    Task IEventSubscriber.HandleEventAsync(string eventName, object? payload)
    {
        return HandleEventAsync(eventName, payload);
    }

    protected virtual async Task HandleEventAsync(string eventName, object? payload)
    {
        if (!_disposed)
        {
            await InvokeAsync(StateHasChanged);
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        foreach (var subscription in _subscriptionIds)
        {
            await UiEventService.UnsubscribeAsync(subscription.Key, subscription.Value);
        }
        _subscriptionIds.Clear();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            foreach (var subscription in _subscriptionIds)
            {
                UiEventService.Unsubscribe(subscription.Key, subscription.Value);
            }
            _subscriptionIds.Clear();
        }

        _disposed = true;
    }

    protected virtual void OnDisposing() { }
}
