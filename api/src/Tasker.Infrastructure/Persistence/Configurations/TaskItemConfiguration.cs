using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasker.Domain.Entities;

namespace Tasker.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("Tasks");
        
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();
        
        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(t => t.Description)
            .HasMaxLength(2000);
        
        builder.Property(t => t.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.Property(t => t.Status)
            .HasConversion<string>()
            .HasMaxLength(20);
        
        builder.Property(t => t.DueDate)
            .IsRequired(false);
        
        builder.Property(t => t.CreatedAt)
            .IsRequired();
        
        builder.Property(t => t.UpdatedAt)
            .IsRequired(false);
        
        // Indexes for common query patterns
        builder.HasIndex(t => t.Status)
            .HasDatabaseName("IX_Tasks_Status");
        
        builder.HasIndex(t => t.Priority)
            .HasDatabaseName("IX_Tasks_Priority");
        
        builder.HasIndex(t => t.DueDate)
            .HasDatabaseName("IX_Tasks_DueDate");
        
        // Composite index for list queries
        builder.HasIndex(t => new { t.Status, t.Priority, t.DueDate })
            .HasDatabaseName("IX_Tasks_Status_Priority_DueDate");
    }
}