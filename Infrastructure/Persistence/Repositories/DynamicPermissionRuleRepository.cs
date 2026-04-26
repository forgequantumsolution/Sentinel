using Microsoft.EntityFrameworkCore;
using Application.Common.Pagination;
using Core.Entities;
using Application.Interfaces.Persistence;

namespace Infrastructure.Persistence.Repositories
{
    public class DynamicGroupObjectPermissionRepository : IDynamicGroupObjectPermissionRepository
    {
        private readonly AppDbContext _context;

        public DynamicGroupObjectPermissionRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<DynamicGroupObjectPermission> BaseQuery()
        {
            return _context.DynamicGroupObjectPermissions
                .Include(r => r.UserGroup)
                .Include(r => r.ActionObjectPermissionSets)
                    .ThenInclude(s => s.ActionObject)
                .Include(r => r.ActionObjectPermissionSets)
                    .ThenInclude(s => s.Permissions)
                        .ThenInclude(p => p.Permission);
        }

        public async Task<DynamicGroupObjectPermission?> GetByIdAsync(Guid id)
        {
            return await BaseQuery().FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<PagedResult<DynamicGroupObjectPermission>> GetAllAsync(PageRequest pageRequest)
        {
            return await PageAsync(BaseQuery(), pageRequest);
        }

        public async Task<PagedResult<DynamicGroupObjectPermission>> GetByUserGroupIdAsync(Guid userGroupId, PageRequest pageRequest)
        {
            return await PageAsync(
                BaseQuery().Where(r => r.UserGroupId == userGroupId),
                pageRequest);
        }

        public async Task<PagedResult<DynamicGroupObjectPermission>> GetByActionObjectIdAsync(Guid actionObjectId, PageRequest pageRequest)
        {
            return await PageAsync(
                BaseQuery().Where(r => r.ActionObjectPermissionSets.Any(s => s.ActionObjectId == actionObjectId)),
                pageRequest);
        }

        public async Task<PagedResult<DynamicGroupObjectPermission>> GetByPermissionIdAsync(Guid permissionId, PageRequest pageRequest)
        {
            return await PageAsync(
                BaseQuery().Where(r => r.ActionObjectPermissionSets.Any(s => s.Permissions.Any(p => p.PermissionId == permissionId))),
                pageRequest);
        }

        public async Task<PagedResult<DynamicGroupObjectPermission>> GetByActionObjectAndPermissionAsync(Guid actionObjectId, Guid permissionId, PageRequest pageRequest)
        {
            return await PageAsync(
                BaseQuery().Where(r => r.ActionObjectPermissionSets.Any(s =>
                    s.ActionObjectId == actionObjectId &&
                    s.Permissions.Any(p => p.PermissionId == permissionId))),
                pageRequest);
        }

        private static async Task<PagedResult<DynamicGroupObjectPermission>> PageAsync(IQueryable<DynamicGroupObjectPermission> query, PageRequest pageRequest)
        {
            var ordered = query.OrderBy(r => r.CreatedAt);
            var totalCount = await ordered.CountAsync();
            var items = await ordered.Skip(pageRequest.Skip).Take(pageRequest.PageSize).ToListAsync();

            return new PagedResult<DynamicGroupObjectPermission>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }

        public async Task<List<DynamicGroupObjectPermission>> GetRootRulesAsync()
        {
            return await BaseQuery().ToListAsync();
        }

        public async Task AddAsync(DynamicGroupObjectPermission rule)
        {
            await _context.DynamicGroupObjectPermissions.AddAsync(rule);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DynamicGroupObjectPermission rule)
        {
            _context.DynamicGroupObjectPermissions.Update(rule);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var rule = await _context.DynamicGroupObjectPermissions.FindAsync(id);
            if (rule != null)
            {
                rule.IsDeleted = true;
                rule.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.DynamicGroupObjectPermissions.AnyAsync(r => r.Id == id);
        }
    }
}
