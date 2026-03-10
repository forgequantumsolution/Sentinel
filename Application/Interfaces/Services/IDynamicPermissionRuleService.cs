using Core.Entities;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IDynamicPermissionRuleService
    {
        Task<DynamicPermissionRuleDto?> GetByIdAsync(Guid id);
        Task<List<DynamicPermissionRuleDto>> GetAllAsync();
        Task<List<DynamicPermissionRuleDto>> GetByUserGroupIdAsync(Guid userGroupId);
        Task<List<DynamicPermissionRuleDto>> GetByActionObjectIdAsync(Guid actionObjectId);
        Task<List<DynamicPermissionRuleDto>> GetByPermissionIdAsync(Guid permissionId);
        Task<List<DynamicPermissionRuleDto>> GetByActionObjectAndPermissionAsync(Guid actionObjectId, Guid permissionId);
        Task<DynamicPermissionRuleDto> CreateAsync(CreateDynamicPermissionRuleRequest request);
        Task<DynamicPermissionRuleDto> UpdateAsync(Guid id, UpdateDynamicPermissionRuleRequest request);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> EvaluatePermissionAsync(Guid ruleId, Guid userId);
    }
}
