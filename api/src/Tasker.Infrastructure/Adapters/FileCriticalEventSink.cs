using Microsoft.Extensions.Logging;
using Tasker.Application.Services.Interfaces;
using Tasker.Domain.Events;

namespace Tasker.Infrastructure.Adapters;

public class FileCriticalEventSink(ILogger<FileCriticalEventSink> logger) : ICriticalEventSink
{
    public Task RecordAsync(HighPriorityTaskChanged taskEvent, CancellationToken cancellationToken = default)
    {
        logger.LogWarning(
            "Critical Task Update: Task {TaskId} ({Title}) set to {Priority} priority. Reason: {Reason}. Occurred at: {OccurredAt}",
            taskEvent.TaskId,
            taskEvent.Title,
            taskEvent.Priority,
            taskEvent.Reason,
            taskEvent.OccurredAt);

        return Task.CompletedTask;
    }
}