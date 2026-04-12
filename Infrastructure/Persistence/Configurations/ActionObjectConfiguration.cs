using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class ActionObjectConfiguration : IEntityTypeConfiguration<ActionObject>
{
    public void Configure(EntityTypeBuilder<ActionObject> builder)
    {
        builder.HasIndex(ao => ao.Code).IsUnique().HasFilter("\"Code\" IS NOT NULL");
        builder.HasIndex(ao => ao.Route).IsUnique().HasFilter("\"Route\" IS NOT NULL");

        builder.HasOne(ao => ao.ParentObject)
              .WithMany(ao => ao.ChildObjects)
              .HasForeignKey(ao => ao.ParentObjectId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
