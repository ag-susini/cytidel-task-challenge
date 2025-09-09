using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tasker.Application.Services.Interfaces;
using Tasker.Infrastructure.Adapters;
using Tasker.Infrastructure.Extensions.Configuration;
using Tasker.Infrastructure.Persistence;
using Tasker.Infrastructure.Persistence.Services;
using Tasker.Infrastructure.Repositories;
using Tasker.Shared.Queries;

namespace Tasker.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add PostgreSQL
        services.AddPostgres(configuration);
        
        // Configure seeding options
        services.Configure<DatabaseSeedingOptions>(options => configuration.GetSection(DatabaseSeedingOptions.SectionName).Bind(options));

        // Add Critical Event Sink (file-based)
        services.AddScoped<ICriticalEventSink, FileCriticalEventSink>();
        
        // Add Queries
        services.AddQueries();
        
        return services;
    }

    private static IServiceCollection AddPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");
        
        if (string.IsNullOrEmpty(connectionString))
            throw new InvalidOperationException("Postgres connection string not found in configuration");
        
        services.AddDbContext<TaskDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(TaskDbContext).Assembly.FullName);
            });
        });

        services.AddScoped<ITaskRepository, TaskItemRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }

    public static async Task InitializeInfrastructureAsync(this IHost host)
    {
        // Apply migrations and seed database
        await host.ApplyMigrationsAsync();
        await host.SeedDatabaseAsync();
    }
    
}