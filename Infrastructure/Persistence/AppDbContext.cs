using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Application.Interfaces;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        private readonly ITenantContext? _tenantContext;

        // EF Core re-evaluates property access on DbContext per query — do NOT inline this
        private Guid? CurrentOrgId => _tenantContext?.OrganizationId;

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
        public DbSet<DynamicGroupObjectPermission> DynamicGroupObjectPermissions { get; set; }
        public DbSet<DynamicForm> DynamicForms { get; set; }
        public DbSet<DynamicFormSubmission> DynamicFormSubmissions { get; set; }
        public DbSet<DynamicFormRecord> DynamicFormRecords { get; set; }
        public DbSet<DynamicFormFieldDefinition> DynamicFormFieldDefinitions { get; set; }
        public DbSet<DynamicFormRecordValue> DynamicFormRecordValues { get; set; }
        public DbSet<DynamicFormDraft> DynamicFormDrafts { get; set; }
        public DbSet<Feature> Features { get; set; }
        public DbSet<AppPermission> AppPermissions { get; set; }
        public DbSet<FeaturePermissionAssignment> FeaturePermissionAssignments { get; set; }
        public DbSet<GraphConfigEntity> GraphConfigs { get; set; }
        public DbSet<GraphDataDefinitionEntity> GraphDataDefinitions { get; set; }
        public DbSet<BulkUploadJob> BulkUploadJobs { get; set; }
        public DbSet<ActionObject> ActionObjects { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<UserGroupMembership> UserGroupMemberships { get; set; }
        public DbSet<ActionObjectPermissionSet> ActionObjectPermissionSets { get; set; }
        public DbSet<ActionObjectPermissionSetItem> ActionObjectPermissionSetItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all IEntityTypeConfiguration<T> from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Auto-apply tenant query filters to all TenantEntity subclasses
            ApplyTenantQueryFilters(modelBuilder);

            // Override Role & User filters to also exclude shadow super-admin
            modelBuilder.Entity<Role>().HasQueryFilter(
                e => (CurrentOrgId == null || e.OrganizationId == CurrentOrgId || e.Organization!.ParentOrganizationId == CurrentOrgId)
                  && e.Name != "super-admin");

            modelBuilder.Entity<User>().HasQueryFilter(
                e => (CurrentOrgId == null || e.OrganizationId == CurrentOrgId || e.Organization!.ParentOrganizationId == CurrentOrgId)
                  && e.Role!.Name != "super-admin");
        }

        private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
        {
            var method = GetType().GetMethod(nameof(ApplyTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)!;

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
                    continue;

                method.MakeGenericMethod(entityType.ClrType).Invoke(this, [modelBuilder]);
            }
        }

        private void ApplyTenantFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : TenantEntity
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(
                e => CurrentOrgId == null
                  || e.OrganizationId == CurrentOrgId
                  || e.Organization!.ParentOrganizationId == CurrentOrgId);
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
