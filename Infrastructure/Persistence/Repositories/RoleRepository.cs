using Application.Common.Pagination;
using Core.Entities;
using Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetByIdAsync(Guid id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task<Role?> GetByNameAsync(string name)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Name == name);
        }

        public async Task<PagedResult<Role>> GetAllAsync(PageRequest pageRequest)
        {
            var query = _context.Roles.OrderBy(r => r.Name);

            var totalCount = await query.CountAsync();
            var items = await query.Skip(pageRequest.Skip).Take(pageRequest.PageSize).ToListAsync();

            return new PagedResult<Role>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }

        public async Task AddAsync(Role role)
        {
            await _context.Roles.AddAsync(role);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Role role)
        {
            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                role.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
