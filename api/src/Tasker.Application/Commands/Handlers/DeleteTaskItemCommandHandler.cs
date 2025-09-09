using Tasker.Application.Commands;
using Tasker.Application.Services.Interfaces;
using Tasker.Shared.Abstractions.Commands;
using Task = System.Threading.Tasks.Task;

namespace Tasker.Application.Commands.Handlers;

public class DeleteTaskItemCommandHandler(ITaskRepository taskRepository, IRealtimeNotifier realtimeNotifier) : ICommandHandler<DeleteTaskItemCommand>
{
    public async Task HandleAsync(DeleteTaskItemCommand command)
    {
        var task = await taskRepository.GetByIdAsync(command.Id);

        if (task == null)
            throw new InvalidOperationException($"Task with ID {command.Id} not found");

        await taskRepository.DeleteAsync(task);
        await taskRepository.SaveChangesAsync();

        // Notify real-time clients
        await realtimeNotifier.NotifyTaskDeletedAsync(command.Id);
    }
}