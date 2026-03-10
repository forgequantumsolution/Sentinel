using Core.Entities;
using Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class GraphDataDefinitionRepository : IGraphDataDefinitionRepository
    {
        private readonly AppDbContext _context;

        public GraphDataDefinitionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<GraphDataDefinitionEntity?> GetByIdAsync(Guid id)
        {
            return await _context.GraphDataDefinitions
                .Include(g => g.GraphConfig)
                .Include(g => g.CreatedBy)
                .Include(g => g.Organization)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<GraphDataDefinitionEntity?> GetByGraphConfigIdAsync(Guid graphConfigId)
        {
            return await _context.GraphDataDefinitions
                .Include(g => g.GraphConfig)
                .FirstOrDefaultAsync(g => g.GraphConfigId == graphConfigId);
        }

        public async Task<List<GraphDataDefinitionEntity>> GetAllAsync()
        {
            return await _context.GraphDataDefinitions
                .Include(g => g.GraphConfig)
                .Include(g => g.CreatedBy)
                .Include(g => g.Organization)
                .Where(g => !g.IsDeleted)
                .ToListAsync();
        }

        public async Task<List<GraphDataDefinitionEntity>> GetByDataSourceTypeAsync(Core.Enums.DataSourceType dataSourceType)
        {
            return await _context.GraphDataDefinitions
                .Where(g => g.Source.Type == dataSourceType && !g.IsDeleted)
                .ToListAsync();
        }

        public async Task AddAsync(GraphDataDefinitionEntity graphDataDefinition)
        {
            await _context.GraphDataDefinitions.AddAsync(graphDataDefinition);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(GraphDataDefinitionEntity graphDataDefinition)
        {
            graphDataDefinition.UpdatedAt = DateTime.UtcNow;
            _context.GraphDataDefinitions.Update(graphDataDefinition);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var graphDataDefinition = await _context.GraphDataDefinitions.FindAsync(id);
            if (graphDataDefinition != null)
            {
                graphDataDefinition.IsDeleted = true;
                graphDataDefinition.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteByGraphConfigIdAsync(Guid graphConfigId)
        {
            var graphDataDefinitions = await _context.GraphDataDefinitions
                .Where(g => g.GraphConfigId == graphConfigId)
                .ToListAsync();
            
            foreach (var graphDataDefinition in graphDataDefinitions)
            {
                graphDataDefinition.IsDeleted = true;
                graphDataDefinition.DeletedAt = DateTime.UtcNow;
            }
            
            await _context.SaveChangesAsync();
        }
    }
}