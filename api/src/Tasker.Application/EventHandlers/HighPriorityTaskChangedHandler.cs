using Tasker.Application.Services.Interfaces;
using Tasker.Domain.Events;

namespace Tasker.Application.EventHandlers;

public class HighPriorityTaskChangedHandler(ICriticalEventSink criticalEventSink, IRealtimeNotifier realtimeNotifier) : IHighPriorityTaskChangedHandler
{
    public async Task HandleAsync(HighPriorityTaskChanged domainEvent, CancellationToken cancellationToken = default)
    {
        await criticalEventSink.RecordAsync(domainEvent, cancellationToken);
        
        // Notify real-time clients about high priority task change
        await realtimeNotifier.NotifyHighPriorityTaskChangedAsync(
            domainEvent.TaskId,
            domainEvent.Title,
            domainEvent.Reason,
            cancellationToken);
    }
}