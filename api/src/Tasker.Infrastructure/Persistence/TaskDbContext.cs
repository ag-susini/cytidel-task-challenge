using Microsoft.EntityFrameworkCore;
using Tasker.Domain.Entities;

namespace Tasker.Infrastructure.Persistence;

public class TaskDbContext(DbContextOptions<TaskDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaskDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<TaskItem>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            // Ensure DueDate is UTC if it exists to prevent PostgreSQL errors
            if (entry.Entity.DueDate.HasValue)
            {
                entry.Entity.DueDate = entry.Entity.DueDate.Value.Kind switch
                {
                    DateTimeKind.Unspecified => DateTime.SpecifyKind(entry.Entity.DueDate.Value, DateTimeKind.Utc),
                    DateTimeKind.Local => entry.Entity.DueDate.Value.ToUniversalTime(),
                    DateTimeKind.Utc => entry.Entity.DueDate.Value,
                    _ => entry.Entity.DueDate.Value
                };
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}