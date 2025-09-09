using Tasker.Application.DTOs;
using Tasker.Shared.Abstractions.Queries;

namespace Tasker.Application.Queries;

public record GetTaskItemStatsQuery : IQuery<TaskStatsDto>;