using Tasker.Domain.Events;

namespace Tasker.Application.EventHandlers;

public interface IHighPriorityTaskChangedHandler
{
    Task HandleAsync(HighPriorityTaskChanged domainEvent, CancellationToken cancellationToken = default);
}