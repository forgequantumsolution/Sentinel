using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Application.Interfaces.Persistence;

namespace Infrastructure.Persistence.Repositories
{
    public class DynamicPermissionRuleRepository : IDynamicPermissionRuleRepository
    {
        private readonly AppDbContext _context;

        public DynamicPermissionRuleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DynamicPermissionRule?> GetByIdAsync(Guid id)
        {
            return await _context.DynamicPermissionRules
                .Include(r => r.UserGroup)
                .Include(r => r.ParentRule)
                .Include(r => r.ChildRules)
                .Include(r => r.ActionObject)
                .Include(r => r.Permission)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<List<DynamicPermissionRule>> GetAllAsync()
        {
            return await _context.DynamicPermissionRules
                .Include(r => r.UserGroup)
                .Include(r => r.ParentRule)
                .Include(r => r.ChildRules)
                .Where(r => r.ParentRuleId == null) // Get root rules by default
                .ToListAsync();
        }

        public async Task<List<DynamicPermissionRule>> GetByUserGroupIdAsync(Guid userGroupId)
        {
            return await _context.DynamicPermissionRules
                .Include(r => r.UserGroup)
                .Include(r => r.ParentRule)
                .Include(r => r.ChildRules)
                .Where(r => r.UserGroupId == userGroupId)
                .ToListAsync();
        }

        public async Task<List<DynamicPermissionRule>> GetByActionObjectIdAsync(Guid actionObjectId)
        {
            return await _context.DynamicPermissionRules
                .Include(r => r.UserGroup)
                .Include(r => r.ParentRule)
                .Include(r => r.ChildRules)
                .Include(r => r.ActionObject)
                .Include(r => r.Permission)
                .Where(r => r.ActionObjectId == actionObjectId)
                .ToListAsync();
        }

        public async Task<List<DynamicPermissionRule>> GetByPermissionIdAsync(Guid permissionId)
        {
            return await _context.DynamicPermissionRules
                .Include(r => r.UserGroup)
                .Include(r => r.ParentRule)
                .Include(r => r.ChildRules)
                .Include(r => r.ActionObject)
                .Include(r => r.Permission)
                .Where(r => r.PermissionId == permissionId)
                .ToListAsync();
        }

        public async Task<List<DynamicPermissionRule>> GetByActionObjectAndPermissionAsync(Guid actionObjectId, Guid permissionId)
        {
            return await _context.DynamicPermissionRules
                .Include(r => r.UserGroup)
                .Include(r => r.ParentRule)
                .Include(r => r.ChildRules)
                .Include(r => r.ActionObject)
                .Include(r => r.Permission)
                .Where(r => r.ActionObjectId == actionObjectId && r.PermissionId == permissionId)
                .ToListAsync();
        }

        public async Task<List<DynamicPermissionRule>> GetRootRulesAsync()
        {
            return await _context.DynamicPermissionRules
                .Include(r => r.UserGroup)
                .Include(r => r.ChildRules)
                .Where(r => r.ParentRuleId == null)
                .ToListAsync();
        }

        public async Task AddAsync(DynamicPermissionRule rule)
        {
            await _context.DynamicPermissionRules.AddAsync(rule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DynamicPermissionRule rule)
        {
            _context.DynamicPermissionRules.Update(rule);
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
                
                _context.DynamicPermissionRules.Remove(rule);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.DynamicPermissionRules.AnyAsync(r => r.Id == id);
        }
    }
}