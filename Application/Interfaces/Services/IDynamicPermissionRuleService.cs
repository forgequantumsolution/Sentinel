using Application.Common.Pagination;
using Core.Entities;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IDynamicGroupObjectPermissionService
    {
        Task<DynamicGroupObjectPermissionDto?> GetByIdAsync(Guid id);
        Task<PagedResult<DynamicGroupObjectPermissionDto>> GetAllAsync(PageRequest pageRequest);
        Task<PagedResult<DynamicGroupObjectPermissionDto>> GetByUserGroupIdAsync(Guid userGroupId, PageRequest pageRequest);
        Task<PagedResult<DynamicGroupObjectPermissionDto>> GetByActionObjectIdAsync(Guid actionObjectId, PageRequest pageRequest);
        Task<PagedResult<DynamicGroupObjectPermissionDto>> GetByPermissionIdAsync(Guid permissionId, PageRequest pageRequest);
        Task<PagedResult<DynamicGroupObjectPermissionDto>> GetByActionObjectAndPermissionAsync(Guid actionObjectId, Guid permissionId, PageRequest pageRequest);
        Task<DynamicGroupObjectPermissionDto> CreateAsync(CreateDynamicGroupObjectPermissionRequest request);
        Task<DynamicGroupObjectPermissionDto> UpdateAsync(Guid id, UpdateDynamicGroupObjectPermissionRequest request);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
