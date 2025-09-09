using Tasker.Domain.Enums;

namespace Tasker.Application.DTOs;

public record TaskDto(
    Guid Id,
    string Title,
    string? Description,
    Priority Priority,
    Status Status,
    DateTime? DueDate,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);