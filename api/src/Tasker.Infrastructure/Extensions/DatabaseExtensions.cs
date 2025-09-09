using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tasker.Infrastructure.Extensions.Configuration;
using Tasker.Infrastructure.Persistence;
using Tasker.Infrastructure.Persistence.Services;

namespace Tasker.Infrastructure.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IHost>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
        
        try
        {
            logger.LogInformation("Applying database migrations...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying database migrations");
            throw;
        }
    }

    public static async Task SeedDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<IHost>>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseSeedingOptions>>().Value;
        
        if (!options.Enabled)
        {
            logger.LogInformation("Database seeding is disabled");
            return;
        }

        try
        {
            logger.LogInformation("Starting database seeding with {TaskCount} tasks (ClearExisting: {ClearExisting})", 
                options.TaskCount, options.ClearExistingData);

            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedTasksAsync(options.TaskCount, options.ClearExistingData);

            logger.LogInformation("Database seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}