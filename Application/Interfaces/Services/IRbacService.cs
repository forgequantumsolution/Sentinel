using Analytics_BE.Core.Entities;
using Analytics_BE.Core.Enums;

namespace Analytics_BE.Application.Interfaces.Services
{
    public interface IRbacService
    {
        // ── Feature CRUD ──
        Task<Feature> CreateFeatureAsync(Feature feature);
        Task<Feature?> GetFeatureByIdAsync(Guid id);
        Task<IEnumerable<Feature>> GetAllFeaturesAsync();
        Task UpdateFeatureAsync(Feature feature);
        Task DeleteFeatureAsync(Guid id);

        // ── AppPermission CRUD ──
        Task<AppPermission> CreatePermissionAsync(AppPermission permission);
        Task<AppPermission?> GetPermissionByIdAsync(Guid id);
        Task<IEnumerable<AppPermission>> GetAllPermissionsAsync();
        Task UpdatePermissionAsync(AppPermission permission);
        Task DeletePermissionAsync(Guid id);

        // ── Assignment ──
        /// <summary>
        /// Assign a feature-permission to an organization or user.
        /// When AssigneeType = User, this will validate that the user's
        /// organization already has the same feature-permission.
        /// </summary>
        Task<FeaturePermissionAssignment> AssignAsync(Guid featureId, Guid permissionId, AssigneeType assigneeType, Guid assigneeId);

        /// <summary>
        /// Revoke a feature-permission from an assignee.
        /// When revoking from an Organization, all user-level assignments
        /// for the same feature-permission within that org are also revoked.
        /// </summary>
        Task RevokeAsync(Guid featureId, Guid permissionId, AssigneeType assigneeType, Guid assigneeId);

        /// <summary>
        /// Check if a user has a specific feature-permission.
        /// </summary>
        Task<bool> UserHasPermissionAsync(Guid userId, string featureCode, string permissionCode);

        /// <summary>
        /// Check if an organization has a specific feature-permission.
        /// </summary>
        Task<bool> OrgHasPermissionAsync(Guid orgId, Guid featureId, Guid permissionId);

        /// <summary>
        /// Get all feature-permission assignments for a given user.
        /// </summary>
        Task<IEnumerable<FeaturePermissionAssignment>> GetUserAssignmentsAsync(Guid userId);

        /// <summary>
        /// Get all feature-permission assignments for a given organization.
        /// </summary>
        Task<IEnumerable<FeaturePermissionAssignment>> GetOrgAssignmentsAsync(Guid orgId);
    }
}
