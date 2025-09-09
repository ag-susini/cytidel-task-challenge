using Tasker.Domain.Enums;

namespace Tasker.Domain.Events;

public record HighPriorityTaskChanged(
    Guid TaskId,
    string Title,
    Priority Priority,
    string Reason,
    DateTime OccurredAt
);