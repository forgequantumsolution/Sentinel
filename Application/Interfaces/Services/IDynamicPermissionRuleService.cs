using Core.Entities;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IDynamicGroupObjectPermissionService
    {
        Task<DynamicGroupObjectPermissionDto?> GetByIdAsync(Guid id);
        Task<List<DynamicGroupObjectPermissionDto>> GetAllAsync();
        Task<List<DynamicGroupObjectPermissionDto>> GetByUserGroupIdAsync(Guid userGroupId);
        Task<List<DynamicGroupObjectPermissionDto>> GetByActionObjectIdAsync(Guid actionObjectId);
        Task<List<DynamicGroupObjectPermissionDto>> GetByPermissionIdAsync(Guid permissionId);
        Task<List<DynamicGroupObjectPermissionDto>> GetByActionObjectAndPermissionAsync(Guid actionObjectId, Guid permissionId);
        Task<DynamicGroupObjectPermissionDto> CreateAsync(CreateDynamicGroupObjectPermissionRequest request);
        Task<DynamicGroupObjectPermissionDto> UpdateAsync(Guid id, UpdateDynamicGroupObjectPermissionRequest request);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}
