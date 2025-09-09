using Tasker.Domain.Entities;

namespace Tasker.Application.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashToken(string token);
}