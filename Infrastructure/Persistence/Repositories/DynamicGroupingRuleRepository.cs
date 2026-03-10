using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Application.Interfaces.Persistence;

namespace Infrastructure.Persistence.Repositories
{
    public class DynamicGroupingRuleRepository : IDynamicGroupingRuleRepository
    {
        private readonly AppDbContext _context;

        public DynamicGroupingRuleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DynamicGroupingRule?> GetByIdAsync(Guid id)
        {
            return await _context.DynamicGroupingRules
                .Include(r => r.UserGroup)
                .Include(r => r.ParentRule)
                .Include(r => r.ChildRules)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<DynamicGroupingRule>> GetAllAsync()
        {
            return await _context.DynamicGroupingRules
                .Include(r => r.UserGroup)
                .Include(r => r.ParentRule)
                .Include(r => r.ChildRules)
                .Where(r => r.ParentRuleId == null) // Get root rules by default
                .ToListAsync();
        }

        public async Task<List<DynamicGroupingRule>> GetByUserGroupIdAsync(Guid userGroupId)
        {
            return await _context.DynamicGroupingRules
                .Include(r => r.UserGroup)
                .Include(r => r.ParentRule)
                .Include(r => r.ChildRules)
                .Where(r => r.UserGroupId == userGroupId)
                .ToListAsync();
        }

        public async Task<List<DynamicGroupingRule>> GetRootRulesAsync()
        {
            return await _context.DynamicGroupingRules
                .Include(r => r.UserGroup)
                .Include(r => r.ChildRules)
                .Where(r => r.ParentRuleId == null)
                .ToListAsync();
        }

        public async Task AddAsync(DynamicGroupingRule rule)
        {
            await _context.DynamicGroupingRules.AddAsync(rule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DynamicGroupingRule rule)
        {
            _context.DynamicGroupingRules.Update(rule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var rule = await GetByIdAsync(id);
            if (rule != null)
            {
                // Handle child rules - either delete them or set their ParentRuleId to null
                if (rule.ChildRules != null && rule.ChildRules.Any())
                {
                    foreach (var child in rule.ChildRules)
                    {
                        child.ParentRuleId = null;
                    }
                }
                
                _context.DynamicGroupingRules.Remove(rule);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.DynamicGroupingRules.AnyAsync(r => r.Id == id);
        }
    }
}