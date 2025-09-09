using Microsoft.EntityFrameworkCore;
using Tasker.Application.Services.Interfaces;
using Tasker.Domain.Entities;
using Tasker.Infrastructure.Persistence;

namespace Tasker.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly TaskDbContext _context;
    
    public UserRepository(TaskDbContext context)
    {
        _context = context;
    }
    
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }
    
    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    public async Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }
    
    public async Task<RefreshToken?> GetActiveRefreshTokenAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.RevokedAt == null);
    }
    
    public async Task<bool> RevokeRefreshTokenAsync(string tokenHash)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && rt.RevokedAt == null);
            
        if (token == null)
            return false;
            
        token.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}