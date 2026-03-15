using Core.Entities;
using Application.Interfaces.Persistence;
using Application.Common.Pagination;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class GraphConfigRepository : IGraphConfigRepository
    {
        private readonly AppDbContext _context;

        public GraphConfigRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GraphConfigEntity?> GetByIdAsync(Guid id)
        {
            return await _context.GraphConfigs
                .Include(g => g.CreatedBy)
                .Include(g => g.Organization)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<GraphConfigEntity?> GetByNameAsync(string name)
        {
            return await _context.GraphConfigs
                .FirstOrDefaultAsync(g => g.Name == name);
        }

        public async Task<PagedResult<GraphConfigEntity>> GetAllAsync(PageRequest pageRequest)
        {
            var query = _context.GraphConfigs
                .Include(g => g.CreatedBy)
                .Include(g => g.Organization)
                .Where(g => !g.IsDeleted)
                .OrderByDescending(g => g.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(pageRequest.Skip)
                .Take(pageRequest.PageSize)
                .ToListAsync();

            return new PagedResult<GraphConfigEntity>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }

        public async Task<PagedResult<GraphConfigEntity>> GetByTypeAsync(Core.Enums.GraphType type, PageRequest pageRequest)
        {
            var query = _context.GraphConfigs
                .Include(g => g.CreatedBy)
                .Include(g => g.Organization)
                .Where(g => g.Type == type && !g.IsDeleted)
                .OrderByDescending(g => g.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip(pageRequest.Skip)
                .Take(pageRequest.PageSize)
                .ToListAsync();

            return new PagedResult<GraphConfigEntity>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }

        public async Task<List<GraphConfigEntity>> GetByActionObjectIdsAsync(List<Guid> actionObjectIds)
        {
            return await _context.GraphConfigs
                .Where(g => g.ActionObjectId != null && actionObjectIds.Contains(g.ActionObjectId.Value) && !g.IsDeleted)
                .ToListAsync();
        }

        public async Task AddAsync(GraphConfigEntity graphConfig)
        {
            await _context.GraphConfigs.AddAsync(graphConfig);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(GraphConfigEntity graphConfig)
        {
            graphConfig.UpdatedAt = DateTime.UtcNow;
            _context.GraphConfigs.Update(graphConfig);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var graphConfig = await _context.GraphConfigs.FindAsync(id);
            if (graphConfig != null)
            {
                graphConfig.IsDeleted = true;
                graphConfig.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}