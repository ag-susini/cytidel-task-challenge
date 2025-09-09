using Tasker.Application.Services;
using Tasker.Application.Services.Interfaces;
using Tasker.Shared.Abstractions.Commands;

namespace Tasker.Application.Commands.Handlers;

public class LogoutCommandHandler(IUserRepository userRepository, ITokenService tokenService)
    : ICommandHandler<LogoutCommand, bool>
{
    public async Task<bool> HandleAsync(LogoutCommand command)
    {
        var hashedToken = tokenService.HashToken(command.RefreshToken);
        return await userRepository.RevokeRefreshTokenAsync(hashedToken);
    }
}