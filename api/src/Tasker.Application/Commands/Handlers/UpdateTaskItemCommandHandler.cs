using Tasker.Application.Commands;
using Tasker.Application.EventHandlers;
using Tasker.Application.Services.Interfaces;
using Tasker.Domain.Enums;
using Tasker.Domain.Events;
using Tasker.Shared.Abstractions.Commands;
using Task = System.Threading.Tasks.Task;

namespace Tasker.Application.Commands.Handlers;

public class UpdateTaskItemCommandHandler(ITaskRepository taskRepository, IHighPriorityTaskChangedHandler eventHandler, IRealtimeNotifier realtimeNotifier)
    : ICommandHandler<UpdateTaskItemCommand>
{
    public async Task HandleAsync(UpdateTaskItemCommand command)
    {
        var task = await taskRepository.GetByIdAsync(command.Id);

        if (task == null)
            throw new InvalidOperationException($"Task with ID {command.Id} not found");

        var wasHighPriority = task.Priority == Priority.High;
        var willBeHighPriority = command.Priority == Priority.High;

        task.Title = command.Title;
        task.Description = command.Description;
        task.Priority = command.Priority;
        task.Status = command.Status;
        task.DueDate = command.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await taskRepository.UpdateAsync(task);
        await taskRepository.SaveChangesAsync();

        // Notify real-time clients
        await realtimeNotifier.NotifyTaskUpdatedAsync(task.Id, task.Title);

        // Handle high priority task events
        if (willBeHighPriority)
        {
            var reason = !wasHighPriority 
                ? "Task priority elevated to High" 
                : "High priority task updated";

            var domainEvent = new HighPriorityTaskChanged(
                command.Id,
                command.Title,
                command.Priority,
                reason,
                DateTime.UtcNow
            );

            await eventHandler.HandleAsync(domainEvent);
        }
    }
}