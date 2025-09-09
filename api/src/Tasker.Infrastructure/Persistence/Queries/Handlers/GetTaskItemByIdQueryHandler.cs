using Microsoft.EntityFrameworkCore;
using Tasker.Application.DTOs;
using Tasker.Application.Queries;
using Tasker.Infrastructure.Persistence;
using Tasker.Shared.Abstractions.Queries;

namespace Tasker.Infrastructure.Persistence.Queries.Handlers;

public class GetTaskItemByIdQueryHandler : IQueryHandler<GetTaskItemByIdQuery, TaskDto?>
{
    private readonly TaskDbContext _dbContext;

    public GetTaskItemByIdQueryHandler(TaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<TaskDto?> HandleAsync(GetTaskItemByIdQuery query)
    {
        var task = await _dbContext.Tasks
            .FirstOrDefaultAsync(t => t.Id == query.Id);

        if (task == null)
            return null;

        return new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.Priority,
            task.Status,
            task.DueDate,
            task.CreatedAt,
            task.UpdatedAt
        );
    }
}