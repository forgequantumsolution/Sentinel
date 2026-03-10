using Core.Entities;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IDynamicGroupingRuleService
    {
        Task<DynamicGroupingRuleDto?> GetByIdAsync(Guid id);
        Task<List<DynamicGroupingRuleDto>> GetAllAsync();
        Task<List<DynamicGroupingRuleDto>> GetByUserGroupIdAsync(Guid userGroupId);
        Task<DynamicGroupingRuleDto> CreateAsync(CreateDynamicGroupingRuleRequest request);
        Task<DynamicGroupingRuleDto> UpdateAsync(Guid id, UpdateDynamicGroupingRuleRequest request);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
        Task<bool> UserMatchesRuleAsync(Guid ruleId, Guid userId);
    }
}