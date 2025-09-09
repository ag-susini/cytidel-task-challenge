using Microsoft.Extensions.Options;
using Tasker.Application.DTOs;
using Tasker.Application.Services;
using Tasker.Application.Services.Interfaces;
using Tasker.Domain.Entities;
using Tasker.Shared.Abstractions.Commands;

namespace Tasker.Application.Commands.Handlers;

public class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtSettings)
    : ICommandHandler<RefreshTokenCommand, AuthResultDto>
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async Task<AuthResultDto> HandleAsync(RefreshTokenCommand command)
    {
        var hashedToken = tokenService.HashToken(command.RefreshToken);
        
        var tokenEntity = await userRepository.GetActiveRefreshTokenAsync(hashedToken);
            
        if (tokenEntity is null || !tokenEntity.IsActive)
        {
            return new AuthResultDto(false, null, null, 0, "Invalid or expired refresh token");
        }
        
        // Rotate refresh token (revoke old, create new)
        await userRepository.RevokeRefreshTokenAsync(hashedToken);
        
        var newRefreshToken = tokenService.GenerateRefreshToken();
        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = tokenEntity.UserId,
            TokenHash = tokenService.HashToken(newRefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
            CreatedAt = DateTime.UtcNow,
            UserAgent = null,
            IpAddress = null
        };
        
        await userRepository.CreateRefreshTokenAsync(newRefreshTokenEntity);
        
        var accessToken = tokenService.GenerateAccessToken(tokenEntity.User);
        
        return new AuthResultDto(
            true,
            accessToken,
            newRefreshToken,
            _jwtSettings.AccessTokenMinutes * 60,
            null);
    }
}