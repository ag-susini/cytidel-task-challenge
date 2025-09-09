using Microsoft.Extensions.Options;
using Tasker.Application.DTOs;
using Tasker.Application.Services;
using Tasker.Application.Services.Interfaces;
using Tasker.Domain.Entities;
using Tasker.Shared.Abstractions.Commands;

namespace Tasker.Application.Commands.Handlers;

public class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHashingService passwordHashingService,
    ITokenService tokenService,
    IOptions<JwtSettings> jwtSettings)
    : ICommandHandler<RegisterCommand, AuthResultDto>
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async Task<AuthResultDto> HandleAsync(RegisterCommand command)
    {
        var existingUser = await userRepository.GetUserByEmailAsync(command.Email);
            
        if (existingUser != null)
        {
            return new AuthResultDto(false, null, null, 0, "User with this email already exists");
        }
        
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email.ToLower(),
            PasswordHash = passwordHashingService.HashPassword(command.Password),
            CreatedAt = DateTime.UtcNow
        };
        
        await userRepository.CreateUserAsync(user);
        
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