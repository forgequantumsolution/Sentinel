using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DynamicFormRecordValueConfiguration : IEntityTypeConfiguration<DynamicFormRecordValue>
{
    public void Configure(EntityTypeBuilder<DynamicFormRecordValue> builder)
    {
        builder.HasIndex(e => new { e.SubmissionId, e.FieldDefinitionId }).IsUnique();
        builder.HasIndex(e => new { e.FormId, e.FieldDefinitionId });
    }
}
