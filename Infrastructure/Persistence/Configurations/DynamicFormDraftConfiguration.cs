using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DynamicFormDraftConfiguration : IEntityTypeConfiguration<DynamicFormDraft>
{
    public void Configure(EntityTypeBuilder<DynamicFormDraft> builder)
    {
        // One active draft per user per form
        builder.HasIndex(d => new { d.FormId, d.CreatedById })
              .IsUnique()
              .HasFilter("\"IsDeleted\" = false");
    }
}
