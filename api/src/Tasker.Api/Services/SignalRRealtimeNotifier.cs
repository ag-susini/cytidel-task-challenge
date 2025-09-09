using Microsoft.AspNetCore.SignalR;
using Tasker.Api.Hubs;
using Tasker.Application.Services.Interfaces;

namespace Tasker.Api.Services;

public class SignalRRealtimeNotifier(IHubContext<TasksHub> hubContext) : IRealtimeNotifier
{
    public async Task NotifyTaskCreatedAsync(Guid taskId, string title, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[SignalR] Sending TaskCreated event: {taskId}, {title}");
        await hubContext.Clients.All.SendAsync("TaskCreated", new { TaskId = taskId, Title = title }, cancellationToken);
    }

    public async Task NotifyTaskUpdatedAsync(Guid taskId, string title, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[SignalR] Sending TaskUpdated event: {taskId}, {title}");
        await hubContext.Clients.All.SendAsync("TaskUpdated", new { TaskId = taskId, Title = title }, cancellationToken);
    }

    public async Task NotifyTaskDeletedAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[SignalR] Sending TaskDeleted event: {taskId}");
        await hubContext.Clients.All.SendAsync("TaskDeleted", new { TaskId = taskId }, cancellationToken);
    }

    public async Task NotifyHighPriorityTaskChangedAsync(Guid taskId, string title, string reason, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"[SignalR] Sending HighPriorityTaskChanged event: {taskId}, {title}, {reason}");
        await hubContext.Clients.All.SendAsync("HighPriorityTaskChanged", new { TaskId = taskId, Title = title, Reason = reason }, cancellationToken);
    }
}