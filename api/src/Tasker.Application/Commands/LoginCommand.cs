using Tasker.Application.DTOs;
using Tasker.Shared.Abstractions.Commands;

namespace Tasker.Application.Commands;

public record LoginCommand(
    string Email,
    string Password) : ICommand<AuthResultDto>;