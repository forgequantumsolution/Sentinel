using Application.Common.Pagination;
using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IDynamicGroupingRuleRepository
    {
        Task<DynamicGroupingRule?> GetByIdAsync(Guid id);
        Task<PagedResult<DynamicGroupingRule>> GetAllAsync(PageRequest pageRequest);
        Task<PagedResult<DynamicGroupingRule>> GetByUserGroupIdAsync(Guid userGroupId, PageRequest pageRequest);
        Task<List<DynamicGroupingRule>> GetRootRulesAsync();
        Task AddAsync(DynamicGroupingRule rule);
        Task UpdateAsync(DynamicGroupingRule rule);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Guid id);
    }
}