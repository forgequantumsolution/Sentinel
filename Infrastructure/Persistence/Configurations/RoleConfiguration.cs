using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasIndex(u => u.Name).IsUnique();

        builder.HasMany(u => u.Users)
              .WithOne(r => r.Role)
              .HasForeignKey(u => u.RoleId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
