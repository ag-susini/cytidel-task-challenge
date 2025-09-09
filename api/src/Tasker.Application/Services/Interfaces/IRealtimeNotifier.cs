namespace Tasker.Application.Services.Interfaces;

public interface IRealtimeNotifier
{
    Task NotifyTaskCreatedAsync(Guid taskId, string title, CancellationToken cancellationToken = default);
    Task NotifyTaskUpdatedAsync(Guid taskId, string title, CancellationToken cancellationToken = default);
    Task NotifyTaskDeletedAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task NotifyHighPriorityTaskChangedAsync(Guid taskId, string title, string reason, CancellationToken cancellationToken = default);
}