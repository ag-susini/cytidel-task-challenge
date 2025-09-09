using Tasker.Domain.Entities;

namespace Tasker.Application.Services.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User> CreateUserAsync(User user);
    Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken refreshToken);
    Task<RefreshToken?> GetActiveRefreshTokenAsync(string tokenHash);
    Task<bool> RevokeRefreshTokenAsync(string tokenHash);
    Task SaveChangesAsync();
}