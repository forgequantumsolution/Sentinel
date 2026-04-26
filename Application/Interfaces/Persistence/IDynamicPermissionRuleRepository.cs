using Application.Common.Pagination;
using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IDynamicGroupObjectPermissionRepository
    {
        Task<DynamicGroupObjectPermission?> GetByIdAsync(Guid id);
        Task<PagedResult<DynamicGroupObjectPermission>> GetAllAsync(PageRequest pageRequest);
        Task<PagedResult<DynamicGroupObjectPermission>> GetByUserGroupIdAsync(Guid userGroupId, PageRequest pageRequest);
        Task<PagedResult<DynamicGroupObjectPermission>> GetByActionObjectIdAsync(Guid actionObjectId, PageRequest pageRequest);
        Task<PagedResult<DynamicGroupObjectPermission>> GetByPermissionIdAsync(Guid permissionId, PageRequest pageRequest);
        Task<PagedResult<DynamicGroupObjectPermission>> GetByActionObjectAndPermissionAsync(Guid actionObjectId, Guid permissionId, PageRequest pageRequest);
        Task<List<DynamicGroupObjectPermission>> GetRootRulesAsync();
        Task AddAsync(DynamicGroupObjectPermission rule);
        Task UpdateAsync(DynamicGroupObjectPermission rule);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
