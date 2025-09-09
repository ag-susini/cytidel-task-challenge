using Tasker.Application.DTOs;
using Tasker.Domain.Enums;
using Tasker.Shared.Abstractions.Commands;

namespace Tasker.Application.Commands;

public record CreateTaskItemCommand(string Title, string? Description, Priority Priority, DateTime? DueDate)
    : ICommand<TaskDto>
{
    public Guid Id { get; } = Guid.NewGuid();
}