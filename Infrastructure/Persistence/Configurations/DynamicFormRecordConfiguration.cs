using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DynamicFormRecordConfiguration : IEntityTypeConfiguration<DynamicFormRecord>
{
    public void Configure(EntityTypeBuilder<DynamicFormRecord> builder)
    {
        builder.Property(r => r.SubmissionId).IsRequired();

        builder.HasOne(r => r.Submission)
              .WithOne(r => r.Record)
              .HasForeignKey<DynamicFormRecord>(r => r.SubmissionId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
