namespace Lisa.Interfaces;

public interface IEventSubscriber
{
    Task HandleEventAsync(string eventName, object? payload);
}