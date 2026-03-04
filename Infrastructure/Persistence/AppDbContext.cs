using Microsoft.EntityFrameworkCore;
using Analytics_BE.Core.Entities;

namespace Analytics_BE.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<DynamicGroupingRule> DynamicGroupingRules { get; set; }
        public DbSet<DynamicPermissionRule> DynamicPermissionRules { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Role configuration
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasIndex(u => u.Name).IsUnique();

                entity.HasMany(u => u.Users)
                      .WithOne(r => r.Role)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.RoleId);

                entity.HasOne(u => u.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(u => u.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.Manager)
                      .WithMany()
                      .HasForeignKey(u => u.ManagerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Department configuration
            modelBuilder.Entity<Department>(entity =>
            {
                entity.HasIndex(d => d.Name).IsUnique();
                entity.HasIndex(d => d.Code).IsUnique();
                
                entity.HasOne(d => d.ParentDepartment)
                      .WithMany(d => d.ChildDepartments)
                      .HasForeignKey(d => d.ParentDepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(d => d.Users)
                      .WithOne(u => u.Department)
                      .HasForeignKey(u => u.DepartmentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // JobTitle configuration
            modelBuilder.Entity<JobTitle>(entity =>
            {
                entity.HasIndex(jt => jt.Title).IsUnique();

                entity.HasMany(jt => jt.Users)
                      .WithOne(u => u.JobTitle)
                      .HasForeignKey(u => u.JobTitleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure JSON/CSV serialization for Permission.Allowed
            modelBuilder.Entity<Permission>()
                .Property(p => p.Allowed)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );
        }
    }
}
