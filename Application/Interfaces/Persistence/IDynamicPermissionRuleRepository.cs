using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IDynamicGroupObjectPermissionRepository
    {
        Task<DynamicGroupObjectPermission?> GetByIdAsync(Guid id);
        Task<List<DynamicGroupObjectPermission>> GetAllAsync();
        Task<List<DynamicGroupObjectPermission>> GetByUserGroupIdAsync(Guid userGroupId);
        Task<List<DynamicGroupObjectPermission>> GetByActionObjectIdAsync(Guid actionObjectId);
        Task<List<DynamicGroupObjectPermission>> GetByPermissionIdAsync(Guid permissionId);
        Task<List<DynamicGroupObjectPermission>> GetByActionObjectAndPermissionAsync(Guid actionObjectId, Guid permissionId);
        Task<List<DynamicGroupObjectPermission>> GetRootRulesAsync();
        Task AddAsync(DynamicGroupObjectPermission rule);
        Task UpdateAsync(DynamicGroupObjectPermission rule);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
