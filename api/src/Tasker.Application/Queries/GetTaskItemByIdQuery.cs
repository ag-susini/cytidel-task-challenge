using Tasker.Application.DTOs;
using Tasker.Shared.Abstractions.Queries;

namespace Tasker.Application.Queries;

public record GetTaskItemByIdQuery(Guid Id) : IQuery<TaskDto?>;