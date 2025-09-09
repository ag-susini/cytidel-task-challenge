using Tasker.Domain.Events;

namespace Tasker.Application.Services.Interfaces;

public interface ICriticalEventSink
{
    Task RecordAsync(HighPriorityTaskChanged taskEvent, CancellationToken cancellationToken = default);
}