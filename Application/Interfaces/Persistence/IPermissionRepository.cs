using Analytics_BE.Core.Entities;
using Analytics_BE.Core.Enums;

namespace Analytics_BE.Application.Interfaces.Persistence
{
    public interface IPermissionRepository
    {
        Task<List<Permission>> GetEffectivePermissionsAsync(Guid userId, Guid? resourceId, PermissionType resourceType);
        Task<bool> HasPermissionAsync(Guid userId, string action, Guid? resourceId, PermissionType resourceType);
        Task AddPermissionAssignmentAsync(Guid userId, Guid permissionId);
        Task AddGroupPermissionAssignmentAsync(Guid groupId, Guid permissionId);
        Task AddDynamicRuleAsync(DynamicPermissionRule rule);
    }
}
