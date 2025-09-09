using Tasker.Shared.Abstractions.Commands;

namespace Tasker.Application.Commands;

public record DeleteTaskItemCommand(Guid Id) : ICommand;