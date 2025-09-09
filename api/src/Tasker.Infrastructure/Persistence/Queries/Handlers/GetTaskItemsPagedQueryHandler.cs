using Microsoft.EntityFrameworkCore;
using Tasker.Application.DTOs;
using Tasker.Application.Queries;
using Tasker.Infrastructure.Extensions;
using Tasker.Infrastructure.Persistence;
using Tasker.Shared.Abstractions.Queries;

namespace Tasker.Infrastructure.Persistence.Queries.Handlers;

public class GetTaskItemsPagedQueryHandler : IQueryHandler<GetTaskItemsPagedQuery, PagedResult<TaskListItem>>
{
    private readonly TaskDbContext _dbContext;

    public GetTaskItemsPagedQueryHandler(TaskDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<TaskListItem>> HandleAsync(GetTaskItemsPagedQuery query)
    {
        var tasksQuery = _dbContext.Tasks
            .FilterByStatus(query.Status)
            .FilterByPriority(query.Priority)
            .FilterBySearch(query.Search)
            .ApplySort(query.Sort);

        var totalCount = await tasksQuery.CountAsync();

        var tasks = await tasksQuery
            .ApplyPaging(query.PageNumber, query.PageSize)
            .Select(t => new TaskListItem(
                t.Id,
                t.Title,
                t.Priority,
                t.Status,
                t.DueDate,
                t.CreatedAt
            ))
            .ToListAsync();

        return new PagedResult<TaskListItem>(
            tasks,
            totalCount,
            query.PageNumber,
            query.PageSize
        );
    }
}