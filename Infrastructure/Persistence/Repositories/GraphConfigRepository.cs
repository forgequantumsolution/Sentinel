using Core.Entities;
using Application.Interfaces.Persistence;
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

        public async Task<List<GraphConfigEntity>> GetAllAsync()
        {
            return await _context.GraphConfigs
                .Include(g => g.CreatedBy)
                .Include(g => g.Organization)
                .Where(g => !g.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<GraphConfigEntity>> GetByTypeAsync(Core.Enums.GraphType type)
        {
            return await _context.GraphConfigs
                .Where(g => g.Type == type && !g.IsDeleted)
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