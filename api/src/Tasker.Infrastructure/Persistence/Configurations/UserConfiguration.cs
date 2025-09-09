using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tasker.Domain.Entities;

namespace Tasker.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        
        builder.HasKey(u => u.Id);
        
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);
            
        builder.HasIndex(u => u.Email)
            .IsUnique();
            
        builder.Property(u => u.PasswordHash)
            .IsRequired();
            
        builder.Property(u => u.CreatedAt)
            .IsRequired();
    }
}