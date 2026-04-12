using Core.Entities;
using Core.Enums;

namespace Application.Interfaces.Services
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(Guid userId, string action, Guid? resourceId = null, PermissionType resourceType = PermissionType.System);
        Task<List<Permission>> GetEffectivePermissionsAsync(Guid userId, Guid? resourceId = null, PermissionType resourceType = PermissionType.System);
        Task AssignPermissionToUserAsync(Guid userId, Guid permissionId);
        Task AssignPermissionToGroupAsync(Guid groupId, Guid permissionId);
        Task<DynamicGroupObjectPermission> CreateDynamicGroupObjectPermissionAsync(DynamicGroupObjectPermission rule);
    }
}
