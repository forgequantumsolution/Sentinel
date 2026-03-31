using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Application.Interfaces;

namespace Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        private readonly ITenantContext? _tenantContext;

        // EF Core captures this field reference in query filters so the value is re-evaluated per query
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
        public DbSet<DynamicPermissionRule> DynamicPermissionRules { get; set; }
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all IEntityTypeConfiguration<T> from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Auto-apply tenant query filters to all TenantEntity subclasses
            ApplyTenantQueryFilters(modelBuilder);
        }

        /// <summary>
        /// Scans all entity types registered in the model that inherit from TenantEntity
        /// and applies a global query filter: e => currentOrgId == null || e.OrganizationId == currentOrgId
        /// </summary>
        private void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
        {
            // Reference 'this.CurrentOrgId' so EF Core re-evaluates the value per query
            var dbContextConstant = Expression.Constant(this);
            var currentOrgIdExpr = Expression.Property(dbContextConstant, nameof(CurrentOrgId));
            var nullGuid = Expression.Constant(null, typeof(Guid?));

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(TenantEntity).IsAssignableFrom(entityType.ClrType))
                    continue;

                // Build: (e) => CurrentOrgId == null || e.OrganizationId == CurrentOrgId || e.Organization.ParentOrganizationId == CurrentOrgId
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var orgIdProperty = Expression.Property(parameter, nameof(TenantEntity.OrganizationId));
                var organizationNav = Expression.Property(parameter, nameof(TenantEntity.Organization));
                var parentOrgIdProperty = Expression.Property(organizationNav, nameof(Organization.ParentOrganizationId));

                var isNull = Expression.Equal(currentOrgIdExpr, nullGuid);
                var isCurrentOrg = Expression.Equal(orgIdProperty, currentOrgIdExpr);
                var isParentOrg = Expression.Equal(parentOrgIdProperty, currentOrgIdExpr);
                var body = Expression.OrElse(isNull, Expression.OrElse(isCurrentOrg, isParentOrg));

                var lambda = Expression.Lambda(body, parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
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
