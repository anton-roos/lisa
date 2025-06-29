namespace Lisa.Application.Interfaces;

public interface IEventSubscriber
{
    Task HandleEventAsync(string eventName, object? payload);
}