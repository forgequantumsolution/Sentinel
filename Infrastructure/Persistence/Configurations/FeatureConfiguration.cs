using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.HasIndex(f => f.Code).IsUnique();

        builder.HasOne(f => f.ParentFeature)
              .WithMany(f => f.ChildFeatures)
              .HasForeignKey(f => f.ParentFeatureId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
