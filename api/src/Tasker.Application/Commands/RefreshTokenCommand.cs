using Tasker.Application.DTOs;
using Tasker.Shared.Abstractions.Commands;

namespace Tasker.Application.Commands;

public record RefreshTokenCommand(
    string RefreshToken) : ICommand<AuthResultDto>;