using Microsoft.EntityFrameworkCore;
using Analytics_BE.Application.Interfaces.Services;
using Analytics_BE.Core.Entities;
using Analytics_BE.Core.Enums;
using Analytics_BE.Infrastructure.Persistence;

namespace Analytics_BE.Infrastructure.Services
{
    public class RbacService : IRbacService
    {
        private readonly AppDbContext _context;

        public RbacService(AppDbContext context)
        {
            _context = context;
        }

        // ════════════════════════════════════════════
        //  Feature CRUD
        // ════════════════════════════════════════════

        public async Task<Feature> CreateFeatureAsync(Feature feature)
        {
            await _context.Features.AddAsync(feature);
            await _context.SaveChangesAsync();
            return feature;
        }

        public async Task<Feature?> GetFeatureByIdAsync(Guid id)
        {
            return await _context.Features
                .Include(f => f.ChildFeatures)
                .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
        }

        public async Task<IEnumerable<Feature>> GetAllFeaturesAsync()
        {
            return await _context.Features
                .Where(f => !f.IsDeleted)
                .Include(f => f.ChildFeatures)
                .OrderBy(f => f.Name)
                .ToListAsync();
        }

        public async Task UpdateFeatureAsync(Feature feature)
        {
            feature.UpdatedAt = DateTime.UtcNow;
            _context.Features.Update(feature);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteFeatureAsync(Guid id)
        {
            var feature = await _context.Features.FindAsync(id);
            if (feature != null)
            {
                feature.IsDeleted = true;
                feature.IsActive = false;
                feature.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // ════════════════════════════════════════════
        //  AppPermission CRUD
        // ════════════════════════════════════════════

        public async Task<AppPermission> CreatePermissionAsync(AppPermission permission)
        {
            await _context.AppPermissions.AddAsync(permission);
            await _context.SaveChangesAsync();
            return permission;
        }

        public async Task<AppPermission?> GetPermissionByIdAsync(Guid id)
        {
            return await _context.AppPermissions
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<IEnumerable<AppPermission>> GetAllPermissionsAsync()
        {
            return await _context.AppPermissions
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task UpdatePermissionAsync(AppPermission permission)
        {
            permission.UpdatedAt = DateTime.UtcNow;
            _context.AppPermissions.Update(permission);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePermissionAsync(Guid id)
        {
            var perm = await _context.AppPermissions.FindAsync(id);
            if (perm != null)
            {
                perm.IsDeleted = true;
                perm.IsActive = false;
                perm.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // ════════════════════════════════════════════
        //  Assignment / Revocation
        // ════════════════════════════════════════════

        public async Task<FeaturePermissionAssignment> AssignAsync(
            Guid featureId, Guid permissionId, AssigneeType assigneeType, Guid assigneeId)
        {
            // ── Business rule: user can only get what their org already has ──
            if (assigneeType == AssigneeType.User)
            {
                var user = await _context.Users
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Id == assigneeId);

                if (user == null)
                    throw new InvalidOperationException("User not found.");

                if (!user.OrganizationId.HasValue)
                    throw new InvalidOperationException("User does not belong to any organization.");

                var orgHasAccess = await _context.FeaturePermissionAssignments
                    .IgnoreQueryFilters()
                    .AnyAsync(a =>
                        a.FeatureId == featureId &&
                        a.PermissionId == permissionId &&
                        a.AssigneeType == AssigneeType.Organization &&
                        a.AssigneeId == user.OrganizationId.Value &&
                        a.IsActive && !a.IsDeleted);

                if (!orgHasAccess)
                    throw new InvalidOperationException(
                        "Cannot assign this feature-permission to the user because " +
                        "their organization does not have access to it.");
            }

            // Check for existing assignment
            var existing = await _context.FeaturePermissionAssignments
                .FirstOrDefaultAsync(a =>
                    a.FeatureId == featureId &&
                    a.PermissionId == permissionId &&
                    a.AssigneeType == assigneeType &&
                    a.AssigneeId == assigneeId && !a.IsDeleted);

            if (existing != null)
            {
                // Reactivate it if it was previously deactivated
                if (!existing.IsActive)
                {
                    existing.IsActive = true;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
                return existing;
            }

            var assignment = new FeaturePermissionAssignment
            {
                FeatureId = featureId,
                PermissionId = permissionId,
                AssigneeType = assigneeType,
                AssigneeId = assigneeId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.FeaturePermissionAssignments.AddAsync(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        public async Task RevokeAsync(
            Guid featureId, Guid permissionId, AssigneeType assigneeType, Guid assigneeId)
        {
            var assignment = await _context.FeaturePermissionAssignments
                .FirstOrDefaultAsync(a =>
                    a.FeatureId == featureId &&
                    a.PermissionId == permissionId &&
                    a.AssigneeType == assigneeType &&
                    a.AssigneeId == assigneeId && !a.IsDeleted);

            if (assignment != null)
            {
                assignment.IsActive = false;
                assignment.IsDeleted = true;
                assignment.DeletedAt = DateTime.UtcNow;
                assignment.UpdatedAt = DateTime.UtcNow;
            }

            // ── Cascade: if revoking from an Org, also revoke from all users in that Org ──
            if (assigneeType == AssigneeType.Organization)
            {
                var userAssignments = await _context.FeaturePermissionAssignments
                    .Where(a =>
                        a.FeatureId == featureId &&
                        a.PermissionId == permissionId &&
                        a.AssigneeType == AssigneeType.User &&
                        a.OrganizationId == assigneeId &&
                        !a.IsDeleted)
                    .ToListAsync();

                foreach (var ua in userAssignments)
                {
                    ua.IsActive = false;
                    ua.IsDeleted = true;
                    ua.DeletedAt = DateTime.UtcNow;
                    ua.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }

        // ════════════════════════════════════════════
        //  Queries
        // ════════════════════════════════════════════

        public async Task<bool> UserHasPermissionAsync(Guid userId, string featureCode, string permissionCode)
        {
            return await _context.FeaturePermissionAssignments
                .Include(a => a.Feature)
                .Include(a => a.Permission)
                .AnyAsync(a =>
                    a.Feature.Code == featureCode &&
                    a.Permission.Code == permissionCode &&
                    a.AssigneeType == AssigneeType.User &&
                    a.AssigneeId == userId &&
                    a.IsActive && !a.IsDeleted);
        }

        public async Task<bool> OrgHasPermissionAsync(Guid orgId, Guid featureId, Guid permissionId)
        {
            return await _context.FeaturePermissionAssignments
                .IgnoreQueryFilters()
                .AnyAsync(a =>
                    a.FeatureId == featureId &&
                    a.PermissionId == permissionId &&
                    a.AssigneeType == AssigneeType.Organization &&
                    a.AssigneeId == orgId &&
                    a.IsActive && !a.IsDeleted);
        }

        public async Task<IEnumerable<FeaturePermissionAssignment>> GetUserAssignmentsAsync(Guid userId)
        {
            return await _context.FeaturePermissionAssignments
                .Include(a => a.Feature)
                .Include(a => a.Permission)
                .Where(a =>
                    a.AssigneeType == AssigneeType.User &&
                    a.AssigneeId == userId &&
                    a.IsActive && !a.IsDeleted)
                .ToListAsync();
        }

        public async Task<IEnumerable<FeaturePermissionAssignment>> GetOrgAssignmentsAsync(Guid orgId)
        {
            return await _context.FeaturePermissionAssignments
                .IgnoreQueryFilters()
                .Include(a => a.Feature)
                .Include(a => a.Permission)
                .Where(a =>
                    a.AssigneeType == AssigneeType.Organization &&
                    a.AssigneeId == orgId &&
                    a.IsActive && !a.IsDeleted)
                .ToListAsync();
        }
    }
}
