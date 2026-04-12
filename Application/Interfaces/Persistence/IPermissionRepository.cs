using Core.Entities;
using Core.Enums;

namespace Application.Interfaces.Persistence
{
    public interface IPermissionRepository
    {
        Task<List<Permission>> GetEffectivePermissionsAsync(Guid userId, Guid? resourceId, PermissionType resourceType);
        Task<bool> HasPermissionAsync(Guid userId, string action, Guid? resourceId, PermissionType resourceType);
        Task AddPermissionAssignmentAsync(Guid userId, Guid permissionId);
        Task AddGroupPermissionAssignmentAsync(Guid groupId, Guid permissionId);
        Task AddDynamicRuleAsync(DynamicGroupObjectPermission rule);
    }
}
