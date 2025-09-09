using Tasker.Domain.Entities;
using Tasker.Domain.Enums;

namespace Tasker.Domain.Factories;

public static class TaskItemFactory
{
    public static TaskItem Create(
        Guid id,
        string title,
        string? description,
        Priority priority,
        DateTime? dueDate)
    {
        return new TaskItem
        {
            Id = id,
            Title = title,
            Description = description,
            Priority = priority,
            DueDate = dueDate,
            Status = Status.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}