using Tasker.Domain.Enums;

namespace Tasker.Application.DTOs;

public record TaskListItem(
    Guid Id,
    string Title,
    Priority Priority,
    Status Status,
    DateTime? DueDate,
    DateTime CreatedAt
);