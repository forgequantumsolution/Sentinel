using Microsoft.EntityFrameworkCore;
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

        public async Task<List<DynamicGroupObjectPermission>> GetAllAsync()
        {
            return await BaseQuery().ToListAsync();
        }

        public async Task<List<DynamicGroupObjectPermission>> GetByUserGroupIdAsync(Guid userGroupId)
        {
            return await BaseQuery()
                .Where(r => r.UserGroupId == userGroupId)
                .ToListAsync();
        }

        public async Task<List<DynamicGroupObjectPermission>> GetByActionObjectIdAsync(Guid actionObjectId)
        {
            return await BaseQuery()
                .Where(r => r.ActionObjectPermissionSets.Any(s => s.ActionObjectId == actionObjectId))
                .ToListAsync();
        }

        public async Task<List<DynamicGroupObjectPermission>> GetByPermissionIdAsync(Guid permissionId)
        {
            return await BaseQuery()
                .Where(r => r.ActionObjectPermissionSets.Any(s => s.Permissions.Any(p => p.PermissionId == permissionId)))
                .ToListAsync();
        }

        public async Task<List<DynamicGroupObjectPermission>> GetByActionObjectAndPermissionAsync(Guid actionObjectId, Guid permissionId)
        {
            return await BaseQuery()
                .Where(r => r.ActionObjectPermissionSets.Any(s =>
                    s.ActionObjectId == actionObjectId &&
                    s.Permissions.Any(p => p.PermissionId == permissionId)))
                .ToListAsync();
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
