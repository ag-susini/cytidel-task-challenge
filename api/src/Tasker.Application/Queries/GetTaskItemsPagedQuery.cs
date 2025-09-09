using Tasker.Application.DTOs;
using Tasker.Domain.Enums;
using Tasker.Shared.Abstractions.Queries;

namespace Tasker.Application.Queries;

public record GetTaskItemsPagedQuery(
    Status? Status = null,
    Priority? Priority = null,
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 10,
    string? Sort = null
) : IQuery<PagedResult<TaskListItem>>;