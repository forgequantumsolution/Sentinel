using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Core.Enums;
using Application.Interfaces.Persistence;
using Application.Common.Pagination;

namespace Infrastructure.Persistence.Repositories
{
    public class ActionObjectRepository : IActionObjectRepository
    {
        private readonly AppDbContext _context;

        public ActionObjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ActionObject?> GetByIdAsync(Guid id)
        {
            return await _context.ActionObjects
                .Include(ao => ao.ParentObject)
                .Include(ao => ao.ChildObjects)
                .FirstOrDefaultAsync(ao => ao.Id == id && !ao.IsDeleted);
        }

        public async Task<List<ActionObject>> GetAllAsync()
        {
            return await _context.ActionObjects
                .Include(ao => ao.ParentObject)
                .Include(ao => ao.ChildObjects)
                .Where(ao => !ao.IsDeleted)
                .OrderBy(ao => ao.SortOrder)
                .ToListAsync();
        }

        public async Task<PagedResult<ActionObject>> GetByTypeAsync(ObjectType objectType, PageRequest pageRequest)
        {
            var query = _context.ActionObjects
                .Include(ao => ao.ParentObject)
                .Include(ao => ao.ChildObjects)
                .Where(ao => ao.ObjectType == objectType && !ao.IsDeleted)
                .OrderBy(ao => ao.SortOrder);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(pageRequest.Skip)
                .Take(pageRequest.PageSize)
                .ToListAsync();

            return new PagedResult<ActionObject>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }

        public async Task<List<ActionObject>> GetRootsByTypeAsync(ObjectType objectType)
        {
            return await _context.ActionObjects
                .Include(ao => ao.ChildObjects)
                .Where(ao => ao.ObjectType == objectType && ao.ParentObjectId == null && !ao.IsDeleted)
                .OrderBy(ao => ao.SortOrder)
                .ToListAsync();
        }

        public async Task<List<ActionObject>> GetChildrenAsync(Guid parentObjectId)
        {
            return await _context.ActionObjects
                .Include(ao => ao.ChildObjects)
                .Where(ao => ao.ParentObjectId == parentObjectId && !ao.IsDeleted)
                .OrderBy(ao => ao.SortOrder)
                .ToListAsync();
        }

        public async Task<PagedResult<ActionObject>> GetChildrenPagedAsync(Guid parentObjectId, PageRequest pageRequest)
        {
            var query = _context.ActionObjects
                .Include(ao => ao.ParentObject)
                .Where(ao => ao.ParentObjectId == parentObjectId && !ao.IsDeleted)
                .OrderBy(ao => ao.SortOrder);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(pageRequest.Skip)
                .Take(pageRequest.PageSize)
                .ToListAsync();

            return new PagedResult<ActionObject>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }

        public async Task<ActionObject?> GetByRouteAsync(string route)
        {
            return await _context.ActionObjects
                .Include(ao => ao.ChildObjects)
                .FirstOrDefaultAsync(ao => ao.Route == route && !ao.IsDeleted);
        }

        public async Task<bool> RouteExistsAsync(string route, Guid? excludeId = null)
        {
            return await _context.ActionObjects
                .AnyAsync(ao => ao.Route == route && !ao.IsDeleted && (excludeId == null || ao.Id != excludeId));
        }

        public async Task AddAsync(ActionObject actionObject)
        {
            _context.ActionObjects.Add(actionObject);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ActionObject actionObject)
        {
            _context.Entry(actionObject).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var obj = await _context.ActionObjects.FindAsync(id);
            if (obj != null)
            {
                obj.IsDeleted = true;
                obj.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
