using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IDynamicGroupingRuleRepository
    {
        Task<DynamicGroupingRule?> GetByIdAsync(Guid id);
        Task<List<DynamicGroupingRule>> GetAllAsync();
        Task<List<DynamicGroupingRule>> GetByUserGroupIdAsync(Guid userGroupId);
        Task<List<DynamicGroupingRule>> GetRootRulesAsync();
        Task AddAsync(DynamicGroupingRule rule);
        Task UpdateAsync(DynamicGroupingRule rule);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}