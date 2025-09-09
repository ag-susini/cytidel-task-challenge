namespace Tasker.Application.DTOs;

public record TaskStatsDto(
    int PendingCount,
    int InProgressCount,
    int CompletedCount,
    int ArchivedCount,
    int TotalCount
);