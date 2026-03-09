using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IGraphDataDefinitionRepository
    {
        Task<GraphDataDefinitionEntity?> GetByIdAsync(Guid id);
        Task<GraphDataDefinitionEntity?> GetByGraphConfigIdAsync(Guid graphConfigId);
        Task<List<GraphDataDefinitionEntity>> GetAllAsync();
        Task<List<GraphDataDefinitionEntity>> GetByDataSourceTypeAsync(Core.Enums.DataSourceType dataSourceType);
        Task AddAsync(GraphDataDefinitionEntity graphDataDefinition);
        Task UpdateAsync(GraphDataDefinitionEntity graphDataDefinition);
        Task DeleteAsync(Guid id);
        Task DeleteByGraphConfigIdAsync(Guid graphConfigId);
    }
}