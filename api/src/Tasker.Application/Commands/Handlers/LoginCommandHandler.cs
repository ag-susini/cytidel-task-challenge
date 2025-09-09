using Microsoft.Extensions.Options;
using Tasker.Application.DTOs;
using Tasker.Application.Services;
using Tasker.Application.Services.Interfaces;
using Tasker.Domain.Entities;
using Tasker.Shared.Abstractions.Commands;

namespace Tasker.Application.Commands.Handlers;

public class LoginCommandHandler(
    IUserRepository userRepository,
    IPasswordHashingService passwordHashingService,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtSettings)
    : ICommandHandler<LoginCommand, AuthResultDto>
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async Task<AuthResultDto> HandleAsync(LoginCommand command)
    {
        var user = await userRepository.GetUserByEmailAsync(command.Email);
            
        if (user == null)
        {
            return new AuthResultDto(false, null, null, 0, "Invalid email or password");
        }
        
        if (!passwordHashingService.VerifyPassword(command.Password, user.PasswordHash))
        {
            return new AuthResultDto(false, null, null, 0, "Invalid email or password");
        }
        
        var refreshToken = tokenService.GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = tokenService.HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenDays),
            CreatedAt = DateTime.UtcNow,
            UserAgent = null,
            IpAddress = null
        };
        
        await userRepository.CreateRefreshTokenAsync(refreshTokenEntity);
        
        var accessToken = tokenService.GenerateAccessToken(user);
        
        return new AuthResultDto(
            true,
            accessToken,
            refreshToken,
            _jwtSettings.AccessTokenMinutes * 60,
            null);
    }
}