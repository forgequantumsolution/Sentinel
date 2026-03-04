using Analytics_BE.Core.Entities;
using Analytics_BE.Core.Enums;

namespace Analytics_BE.Application.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(Guid userId, string action, Guid? resourceId = null, PermissionType resourceType = PermissionType.System);
        Task<List<Permission>> GetEffectivePermissionsAsync(Guid userId, Guid? resourceId = null, PermissionType resourceType = PermissionType.System);
        Task AssignPermissionToUserAsync(Guid userId, Guid permissionId);
        Task AssignPermissionToGroupAsync(Guid groupId, Guid permissionId);
        Task<DynamicPermissionRule> CreateDynamicPermissionRuleAsync(DynamicPermissionRule rule);
    }
}
