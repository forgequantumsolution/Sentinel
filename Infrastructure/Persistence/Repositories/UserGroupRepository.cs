using Analytics_BE.Core.Entities;
using Analytics_BE.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Analytics_BE.Infrastructure.Persistence.Repositories
{
    public class UserGroupRepository : IUserGroupRepository
    {
        private readonly AppDbContext _context;

        public UserGroupRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserGroup?> GetByIdAsync(Guid id)
        {
            return await _context.UserGroups
                .Include(g => g.DynamicGroupingRules)
                .Include(g => g.DynamicPermissionRules)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<List<UserGroup>> GetAllAsync()
        {
            return await _context.UserGroups.ToListAsync();
        }

        public async Task<List<UserGroup>> GetAllWithRulesAsync()
        {
            return await _context.UserGroups
                .Include(g => g.DynamicGroupingRules)
                .Include(g => g.DynamicPermissionRules)
                .ToListAsync();
        }

        public async Task AddAsync(UserGroup group)
        {
            await _context.UserGroups.AddAsync(group);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserGroup group)
        {
            _context.UserGroups.Update(group);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var group = await _context.UserGroups.FindAsync(id);
            if (group != null)
            {
                group.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
