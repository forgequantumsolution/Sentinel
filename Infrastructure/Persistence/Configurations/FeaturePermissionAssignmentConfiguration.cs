using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class FeaturePermissionAssignmentConfiguration : IEntityTypeConfiguration<FeaturePermissionAssignment>
{
    public void Configure(EntityTypeBuilder<FeaturePermissionAssignment> builder)
    {
        builder.HasIndex(a => new { a.FeatureId, a.PermissionId, a.AssigneeType, a.AssigneeId })
              .IsUnique();

        builder.HasOne(a => a.Feature)
              .WithMany(f => f.Assignments)
              .HasForeignKey(a => a.FeatureId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Permission)
              .WithMany(p => p.Assignments)
              .HasForeignKey(a => a.PermissionId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
