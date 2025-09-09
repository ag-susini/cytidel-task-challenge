using Tasker.Domain.Enums;
using Tasker.Shared.Abstractions;
using Tasker.Shared.Abstractions.Commands;

namespace Tasker.Application.Commands;

public record UpdateTaskItemCommand(
    Guid Id,
    string Title,
    string? Description,
    Priority Priority,
    Status Status,
    DateTime? DueDate
) : ICommand;