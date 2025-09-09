using Tasker.Application.DTOs;
using Tasker.Shared.Abstractions.Commands;

namespace Tasker.Application.Commands;

public record RegisterCommand(
    string Email,
    string Password) : ICommand<AuthResultDto>;