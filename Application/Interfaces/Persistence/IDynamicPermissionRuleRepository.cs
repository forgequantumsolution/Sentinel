using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IDynamicPermissionRuleRepository
    {
        Task<DynamicPermissionRule?> GetByIdAsync(Guid id);
        Task<List<DynamicPermissionRule>> GetAllAsync();
        Task<List<DynamicPermissionRule>> GetByUserGroupIdAsync(Guid userGroupId);
        Task<List<DynamicPermissionRule>> GetByActionObjectIdAsync(Guid actionObjectId);
        Task<List<DynamicPermissionRule>> GetByPermissionIdAsync(Guid permissionId);
        Task<List<DynamicPermissionRule>> GetByActionObjectAndPermissionAsync(Guid actionObjectId, Guid permissionId);
        Task<List<DynamicPermissionRule>> GetRootRulesAsync();
        Task AddAsync(DynamicPermissionRule rule);
        Task UpdateAsync(DynamicPermissionRule rule);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
