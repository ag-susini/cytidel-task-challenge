using Tasker.Shared.Abstractions.Commands;

namespace Tasker.Application.Commands;

public record LogoutCommand(string RefreshToken) : ICommand<bool>;