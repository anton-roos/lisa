using lisa_new.Domain.Events;
using Microsoft.Extensions.Logging;

namespace lisa_new.Application.TodoItems.EventHandlers;

public class TodoItemCompletedEventHandler : INotificationHandler<TodoItemCompletedEvent>
{
    private readonly ILogger<TodoItemCompletedEventHandler> _logger;

    public TodoItemCompletedEventHandler(ILogger<TodoItemCompletedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(TodoItemCompletedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("lisa_new Domain Event: {DomainEvent}", notification.GetType().Name);

        return Task.CompletedTask;
    }
}
