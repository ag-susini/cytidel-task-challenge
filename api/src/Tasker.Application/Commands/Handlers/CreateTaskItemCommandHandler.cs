using Tasker.Application.DTOs;
using Tasker.Application.EventHandlers;
using Tasker.Application.Services.Interfaces;
using Tasker.Domain.Enums;
using Tasker.Domain.Events;
using Tasker.Domain.Factories;
using Tasker.Shared.Abstractions.Commands;

namespace Tasker.Application.Commands.Handlers;

public class CreateTaskItemCommandHandler(ITaskRepository taskRepository, IHighPriorityTaskChangedHandler eventHandler, IRealtimeNotifier realtimeNotifier)
    : ICommandHandler<CreateTaskItemCommand, TaskDto>
{
    public async Task<TaskDto> HandleAsync(CreateTaskItemCommand command)
    {
        var task = TaskItemFactory.Create(
            command.Id,
            command.Title,
            command.Description,
            command.Priority,
            command.DueDate);

        await taskRepository.AddAsync(task);
        await taskRepository.SaveChangesAsync();

        // Notify real-time clients
        await realtimeNotifier.NotifyTaskCreatedAsync(task.Id, task.Title);

        // Handle high-priority task event
        if (command.Priority == Priority.High)
        {
            var domainEvent = new HighPriorityTaskChanged(
                task.Id,
                command.Title,
                command.Priority,
                "Task created with high priority",
                DateTime.UtcNow
            );

            await eventHandler.HandleAsync(domainEvent);
        }

        return new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.Priority,
            task.Status,
            task.DueDate,
            task.CreatedAt,
            task.UpdatedAt
        );
    }
}