using Microsoft.AspNetCore.Identity;
using Tasker.Domain.Entities;

namespace Tasker.Application.Services;

public class PasswordHashingService : IPasswordHashingService
{
    private readonly IPasswordHasher<User> _passwordHasher;
    
    public PasswordHashingService()
    {
        _passwordHasher = new PasswordHasher<User>();
    }
    
    public string HashPassword(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }
    
    public bool VerifyPassword(string password, string hashedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(null!, hashedPassword, password);
        return result == PasswordVerificationResult.Success || 
               result == PasswordVerificationResult.SuccessRehashNeeded;
    }
}