using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class JobTitleConfiguration : IEntityTypeConfiguration<JobTitle>
{
    public void Configure(EntityTypeBuilder<JobTitle> builder)
    {
        builder.HasIndex(jt => jt.Title).IsUnique();

        builder.HasMany(jt => jt.Users)
              .WithOne(u => u.JobTitle)
              .HasForeignKey(u => u.JobTitleId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
