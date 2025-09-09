using Microsoft.EntityFrameworkCore;
using Tasker.Application.DTOs;
using Tasker.Application.Queries;
using Tasker.Domain.Enums;
using Tasker.Infrastructure.Persistence;
using Tasker.Shared.Abstractions.Queries;

namespace Tasker.Infrastructure.Persistence.Queries.Handlers;

public class GetTaskItemStatsQueryHandler : IQueryHandler<GetTaskItemStatsQuery, TaskStatsDto>
{
    private readonly TaskDbContext _dbContext;

    public GetTaskItemStatsQueryHandler(TaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskStatsDto> HandleAsync(GetTaskItemStatsQuery query)
    {
        var stats = await _dbContext.Tasks
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var pendingCount = stats.FirstOrDefault(s => s.Status == Status.Pending)?.Count ?? 0;
        var inProgressCount = stats.FirstOrDefault(s => s.Status == Status.InProgress)?.Count ?? 0;
        var completedCount = stats.FirstOrDefault(s => s.Status == Status.Completed)?.Count ?? 0;
        var archivedCount = stats.FirstOrDefault(s => s.Status == Status.Archived)?.Count ?? 0;

        return new TaskStatsDto(
            pendingCount,
            inProgressCount,
            completedCount,
            archivedCount,
            pendingCount + inProgressCount + completedCount + archivedCount
        );
    }
}