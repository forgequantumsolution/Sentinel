using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasIndex(d => d.Name).IsUnique();
        builder.HasIndex(d => d.Code).IsUnique();

        builder.HasOne(d => d.ParentDepartment)
              .WithMany(d => d.ChildDepartments)
              .HasForeignKey(d => d.ParentDepartmentId)
              .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(d => d.Users)
              .WithOne(u => u.Department)
              .HasForeignKey(u => u.DepartmentId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
