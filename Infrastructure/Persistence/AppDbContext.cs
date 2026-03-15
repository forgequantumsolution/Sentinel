using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Core.Entities;
using Core.Models;
using Application.Interfaces;

namespace Infrastructure.Persistence
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
        public DbSet<DynamicFormRecordValue> DynamicFormRecordValues { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<AppPermission> AppPermissions { get; set; }
        public DbSet<FeaturePermissionAssignment> FeaturePermissionAssignments { get; set; }
        public DbSet<GraphConfigEntity> GraphConfigs { get; set; }
        public DbSet<GraphDataDefinitionEntity> GraphDataDefinitions { get; set; }
        public DbSet<BulkUploadJob> BulkUploadJobs { get; set; }
        public DbSet<ActionObject> ActionObjects { get; set; }

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

            modelBuilder.Entity<DynamicFormRecordValue>()
                .HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<Permission>()
                .HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<Feature>();
                //.HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<AppPermission>();
                //.HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<FeaturePermissionAssignment>()
                .HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<GraphConfigEntity>()
                .HasQueryFilter(e => currentOrgId == null || e.OrganizationId == currentOrgId);

            modelBuilder.Entity<GraphDataDefinitionEntity>()
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

            // Configure DynamicFormRecord to have 75 generic fields
            modelBuilder.Entity<DynamicFormRecord>(entity =>
            {
                entity.Property(r=>r.SubmissionId).IsRequired();

                entity.HasOne(r => r.Submission)
                      .WithOne(r => r.Record)
                      .HasForeignKey<DynamicFormRecord>(r => r.SubmissionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure DynamicFormRecordValue (EAV)
            modelBuilder.Entity<DynamicFormRecordValue>(entity =>
            {
                entity.HasIndex(e => new { e.SubmissionId, e.FieldDefinitionId }).IsUnique();
                entity.HasIndex(e => new { e.FormId, e.FieldDefinitionId });
            });

            // Configure ActionObject
            modelBuilder.Entity<ActionObject>(entity =>
            {
                entity.HasIndex(ao => ao.Route).IsUnique().HasFilter("\"Route\" IS NOT NULL");

                entity.HasOne(ao => ao.ParentObject)
                      .WithMany(ao => ao.ChildObjects)
                      .HasForeignKey(ao => ao.ParentObjectId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure GraphConfigEntity — serialize JSON as string, DB-agnostic
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            modelBuilder.Entity<GraphConfigEntity>(entity =>
            {
                entity.Property(e => e.View)
                    .HasColumnName("View")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<GraphViewConfig>(v, jsonOptions) ?? new(),
                        new ValueComparer<GraphViewConfig>(
                            (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
                            v => JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                            v => JsonSerializer.Deserialize<GraphViewConfig>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions) ?? new()));

                entity.Property(e => e.Data)
                    .HasColumnName("Data")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<GraphDataConfig>(v, jsonOptions) ?? new(),
                        new ValueComparer<GraphDataConfig>(
                            (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
                            v => JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                            v => JsonSerializer.Deserialize<GraphDataConfig>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions) ?? new()));

                entity.Property(e => e.Meta)
                    .HasColumnName("Meta")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => v == null ? null : JsonSerializer.Serialize(v, jsonOptions),
                        v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, jsonOptions),
                        new ValueComparer<Dictionary<string, object>?>(
                            (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
                            v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                            v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions)));
            });

            // Configure GraphDataDefinitionEntity — serialize JSON as string
            modelBuilder.Entity<GraphDataDefinitionEntity>(entity =>
            {
                entity.Property(e => e.Source)
                    .HasColumnName("Source")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<DataSourceDefinition>(v, jsonOptions) ?? new(),
                        new ValueComparer<DataSourceDefinition>(
                            (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
                            v => JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                            v => JsonSerializer.Deserialize<DataSourceDefinition>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions) ?? new()));

                entity.Property(e => e.SeriesCalculations)
                    .HasColumnName("SeriesCalculations")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, jsonOptions),
                        v => JsonSerializer.Deserialize<List<SeriesCalculation>>(v, jsonOptions) ?? new(),
                        new ValueComparer<List<SeriesCalculation>>(
                            (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
                            v => JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                            v => JsonSerializer.Deserialize<List<SeriesCalculation>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions) ?? new()));

                entity.Property(e => e.GlobalFilter)
                    .HasColumnName("GlobalFilter")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => v == null ? null : JsonSerializer.Serialize(v, jsonOptions),
                        v => v == null ? null : JsonSerializer.Deserialize<FilterGroup>(v, jsonOptions),
                        new ValueComparer<FilterGroup?>(
                            (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
                            v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                            v => v == null ? null : JsonSerializer.Deserialize<FilterGroup>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions)));

                entity.Property(e => e.SortRules)
                    .HasColumnName("SortRules")
                    .HasColumnType("jsonb")
                    .HasConversion(
                        v => v == null ? null : JsonSerializer.Serialize(v, jsonOptions),
                        v => v == null ? null : JsonSerializer.Deserialize<List<SortRule>>(v, jsonOptions),
                        new ValueComparer<List<SortRule>?>(
                            (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
                            v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                            v => v == null ? null : JsonSerializer.Deserialize<List<SortRule>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions)));
            });

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

