using Core.Entities;
using Core.Models;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class GraphService : IGraphService
    {
        private readonly IGraphConfigRepository _graphConfigRepository;
        private readonly IGraphDataDefinitionRepository _graphDataDefinitionRepository;

        public GraphService(
            IGraphConfigRepository graphConfigRepository,
            IGraphDataDefinitionRepository graphDataDefinitionRepository)
        {
            _graphConfigRepository = graphConfigRepository;
            _graphDataDefinitionRepository = graphDataDefinitionRepository;
        }

        // GraphConfig operations
        public async Task<GraphConfigEntity?> GetGraphConfigByIdAsync(Guid id)
        {
            return await _graphConfigRepository.GetByIdAsync(id);
        }

        public async Task<GraphConfigEntity?> GetGraphConfigByNameAsync(string name)
        {
            return await _graphConfigRepository.GetByNameAsync(name);
        }

        public async Task<List<GraphConfigEntity>> GetAllGraphConfigsAsync()
        {
            return await _graphConfigRepository.GetAllAsync();
        }

        public async Task<List<GraphConfigEntity>> GetGraphConfigsByTypeAsync(Core.Enums.GraphType type)
        {
            return await _graphConfigRepository.GetByTypeAsync(type);
        }

        public async Task<GraphConfigEntity> CreateGraphConfigAsync(GraphConfigDto graphConfigDto)
        {
            var graphConfig = new GraphConfigEntity
            {
                Name = graphConfigDto.Name,
                Type = (Core.Enums.GraphType)graphConfigDto.Type,
                View = graphConfigDto.View,
                Data = graphConfigDto.Data,
                Meta = graphConfigDto.Meta,
                IsActive = graphConfigDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _graphConfigRepository.AddAsync(graphConfig);
            return graphConfig;
        }

        public async Task UpdateGraphConfigAsync(Guid id, GraphConfigDto graphConfigDto)
        {
            var graphConfig = await _graphConfigRepository.GetByIdAsync(id);
            if (graphConfig == null)
                throw new KeyNotFoundException($"GraphConfig with id {id} not found");

            graphConfig.Name = graphConfigDto.Name;
            graphConfig.Type = (Core.Enums.GraphType)graphConfigDto.Type;
            graphConfig.View = graphConfigDto.View;
            graphConfig.Data = graphConfigDto.Data;
            graphConfig.Meta = graphConfigDto.Meta;
            graphConfig.IsActive = graphConfigDto.IsActive;
            graphConfig.UpdatedAt = DateTime.UtcNow;

            await _graphConfigRepository.UpdateAsync(graphConfig);
        }

        public async Task DeleteGraphConfigAsync(Guid id)
        {
            // First delete associated data definitions
            await _graphDataDefinitionRepository.DeleteByGraphConfigIdAsync(id);
            // Then delete the config
            await _graphConfigRepository.DeleteAsync(id);
        }

        // GraphDataDefinition operations
        public async Task<GraphDataDefinitionEntity?> GetGraphDataDefinitionByIdAsync(Guid id)
        {
            return await _graphDataDefinitionRepository.GetByIdAsync(id);
        }

        public async Task<GraphDataDefinitionEntity?> GetGraphDataDefinitionByGraphConfigIdAsync(Guid graphConfigId)
        {
            return await _graphDataDefinitionRepository.GetByGraphConfigIdAsync(graphConfigId);
        }

        public async Task<List<GraphDataDefinitionEntity>> GetAllGraphDataDefinitionsAsync()
        {
            return await _graphDataDefinitionRepository.GetAllAsync();
        }

        public async Task<GraphDataDefinitionEntity> CreateGraphDataDefinitionAsync(GraphDataDefinitionDto graphDataDefinitionDto)
        {
            var graphDataDefinition = new GraphDataDefinitionEntity
            {
                GraphConfigId = graphDataDefinitionDto.GraphConfigId.ToString(),
                Source = graphDataDefinitionDto.Source,
                SeriesCalculations = graphDataDefinitionDto.SeriesCalculations,
                GlobalFilter = graphDataDefinitionDto.GlobalFilter,
                SortRules = graphDataDefinitionDto.SortRules,
                RowLimit = graphDataDefinitionDto.RowLimit,
                IsActive = graphDataDefinitionDto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _graphDataDefinitionRepository.AddAsync(graphDataDefinition);
            return graphDataDefinition;
        }

        public async Task UpdateGraphDataDefinitionAsync(Guid id, GraphDataDefinitionDto graphDataDefinitionDto)
        {
            var graphDataDefinition = await _graphDataDefinitionRepository.GetByIdAsync(id);
            if (graphDataDefinition == null)
                throw new KeyNotFoundException($"GraphDataDefinition with id {id} not found");

            graphDataDefinition.GraphConfigId = graphDataDefinitionDto.GraphConfigId.ToString();
            graphDataDefinition.Source = graphDataDefinitionDto.Source;
            graphDataDefinition.SeriesCalculations = graphDataDefinitionDto.SeriesCalculations;
            graphDataDefinition.GlobalFilter = graphDataDefinitionDto.GlobalFilter;
            graphDataDefinition.SortRules = graphDataDefinitionDto.SortRules;
            graphDataDefinition.RowLimit = graphDataDefinitionDto.RowLimit;
            graphDataDefinition.IsActive = graphDataDefinitionDto.IsActive;
            graphDataDefinition.UpdatedAt = DateTime.UtcNow;

            await _graphDataDefinitionRepository.UpdateAsync(graphDataDefinition);
        }

        public async Task DeleteGraphDataDefinitionAsync(Guid id)
        {
            await _graphDataDefinitionRepository.DeleteAsync(id);
        }

        public async Task DeleteGraphDataDefinitionsByGraphConfigIdAsync(Guid graphConfigId)
        {
            await _graphDataDefinitionRepository.DeleteByGraphConfigIdAsync(graphConfigId);
        }

        // Combined operations
        public async Task<GraphPayload> GetGraphPayloadAsync(Guid graphConfigId)
        {
            var graphConfig = await _graphConfigRepository.GetByIdAsync(graphConfigId);
            if (graphConfig == null)
                throw new KeyNotFoundException($"GraphConfig with id {graphConfigId} not found");

            var graphDataDefinition = await _graphDataDefinitionRepository.GetByGraphConfigIdAsync(graphConfigId);

            var payload = new GraphPayload
            {
                Id = graphConfig.Id.ToString(),
                Type = graphConfig.Type,
                View = graphConfig.View,
                Data = graphConfig.Data,
                Meta = graphConfig.Meta
            };

            // If we have a data definition, we could merge or enhance the payload
            // For now, just return the basic payload

            return payload;
        }

        public async Task<GraphPayload> ExecuteGraphAsync(Guid graphConfigId, Dictionary<string, object>? parameters = null)
        {
            // This would execute the graph query based on the data definition
            // For now, just return the payload without executing the query
            // In a real implementation, this would:
            // 1. Get the graph config and data definition
            // 2. Execute the query based on the data source type
            // 3. Transform the results into the graph data format
            // 4. Return the complete graph payload with actual data
            
            return await GetGraphPayloadAsync(graphConfigId);
        }
    }
}