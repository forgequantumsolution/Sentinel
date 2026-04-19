using Core.Entities;
using Application.Common.Pagination;
using Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
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
                .Include(g => g.DynamicGroupObjectPermissions)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<List<UserGroup>> GetAllAsync()
        {
            return await _context.UserGroups.ToListAsync();
        }

        public async Task<PagedResult<UserGroup>> GetAllAsync(PageRequest pageRequest)
        {
            var hiddenRole = new string[] { "super-admin Role", "sys-admin Role" };
            var query = _context.UserGroups
                .Where(g => !g.IsDeleted && g.Type != Core.Enums.GroupType.Organization && !hiddenRole.Contains(g.Name))
                .OrderByDescending(g => g.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query.Skip(pageRequest.Skip).Take(pageRequest.PageSize).ToListAsync();

            return new PagedResult<UserGroup>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }

        public async Task<List<UserGroup>> GetAllWithRulesAsync()
        {
            return await _context.UserGroups
                .Include(g => g.DynamicGroupingRules)
                .Include(g => g.DynamicGroupObjectPermissions)
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
