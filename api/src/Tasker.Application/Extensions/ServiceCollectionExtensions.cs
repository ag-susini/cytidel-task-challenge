using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tasker.Application.EventHandlers;
using Tasker.Application.Services;
using Tasker.Shared.Commands;

namespace Tasker.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCommands();
        
        services.AddScoped<IHighPriorityTaskChangedHandler, HighPriorityTaskChangedHandler>();
        
        // Configure JWT settings and Auth services
        services.Configure<JwtSettings>(options => configuration.GetSection("Jwt").Bind(options));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHashingService, PasswordHashingService>();
        
        return services;
    }
}