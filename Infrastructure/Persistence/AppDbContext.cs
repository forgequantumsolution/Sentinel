using Microsoft.EntityFrameworkCore;
using Analytics_BE.Core.Entities;
using Analytics_BE.Application.Interfaces;

namespace Analytics_BE.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        private readonly ITenantContext? _tenantContext;

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext? tenantContext = null) : base(options)
        {
            _tenantContext = tenantContext;
        }

        public DbSet<Organization> Organizations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
        public DbSet<UserGroup> UserGroups { get; set; }
        public DbSet<DynamicGroupingRule> DynamicGroupingRules { get; set; }
        public DbSet<DynamicPermissionRule> DynamicPermissionRules { get; set; }
        public DbSet<DynamicForm> DynamicForms { get; set; }
        public DbSet<DynamicFormSubmission> DynamicFormSubmissions { get; set; }
        public DbSet<DynamicFormRecord> DynamicFormRecords { get; set; }
        public DbSet<DynamicFormFieldDefinition> DynamicFormFieldDefinitions { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<AppPermission> AppPermissions { get; set; }
        public DbSet<FeaturePermissionAssignment> FeaturePermissionAssignments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Global query filters for multi-tenancy ──
            var currentOrgId = _tenantContext?.OrganizationId;

            modelBuilder.Entity<User>()
                .HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<Role>()
                .HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<Department>()
                .HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<JobTitle>()
                .HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<UserGroup>()
                .HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<DynamicForm>()
                .HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<DynamicFormSubmission>()
                .HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<DynamicFormRecord>()
                .HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<DynamicFormFieldDefinition>()
                .HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<Permission>()
                .HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<Feature>();
                //.HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<AppPermission>();
                //.HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<FeaturePermissionAssignment>()
                .HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            // ── Existing configurations ──

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

            // ── RBAC configurations ──

            // Feature: unique code, hierarchical parent-child
            modelBuilder.Entity<Feature>(entity =>
            {
                entity.HasIndex(f => f.Code).IsUnique();

                entity.HasOne(f => f.ParentFeature)
                      .WithMany(f => f.ChildFeatures)
                      .HasForeignKey(f => f.ParentFeatureId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // AppPermission: unique code
            modelBuilder.Entity<AppPermission>(entity =>
            {
                entity.HasIndex(p => p.Code).IsUnique();
            });

            // FeaturePermissionAssignment: composite unique index to prevent duplicate grants
            modelBuilder.Entity<FeaturePermissionAssignment>(entity =>
            {
                entity.HasIndex(a => new { a.FeatureId, a.PermissionId, a.AssigneeType, a.AssigneeId })
                      .IsUnique();

                entity.HasOne(a => a.Feature)
                      .WithMany(f => f.Assignments)
                      .HasForeignKey(a => a.FeatureId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Permission)
                      .WithMany(p => p.Assignments)
                      .HasForeignKey(a => a.PermissionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }

        // Auto-set OrganizationId on new entities
        public override int SaveChanges()
        {
            SetOrganizationIdOnNewEntities();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetOrganizationIdOnNewEntities();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void SetOrganizationIdOnNewEntities()
        {
            if (_tenantContext?.OrganizationId == null) return;

            var entries = ChangeTracker.Entries<TenantEntity>()
                .Where(e => e.State == EntityState.Added && e.Entity.OrganizationId == null);

            foreach (var entry in entries)
            {
                entry.Entity.OrganizationId = _tenantContext.OrganizationId;
            }
        }
    }
}

