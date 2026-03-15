using Core.Entities;
using Core.Enums;
using Core.Models;
using Application.DTOs;
using Application.Common.Pagination;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class GraphService : IGraphService
    {
        private readonly IGraphConfigRepository _graphConfigRepository;
        private readonly IGraphDataDefinitionRepository _graphDataDefinitionRepository;
        private readonly IActionObjectRepository _actionObjectRepository;

        public GraphService(
            IGraphConfigRepository graphConfigRepository,
            IGraphDataDefinitionRepository graphDataDefinitionRepository,
            IActionObjectRepository actionObjectRepository)
        {
            _graphConfigRepository = graphConfigRepository;
            _graphDataDefinitionRepository = graphDataDefinitionRepository;
            _actionObjectRepository = actionObjectRepository;
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

        public async Task<PagedResult<GraphConfigEntity>> GetAllGraphConfigsAsync(PageRequest pageRequest)
        {
            return await _graphConfigRepository.GetAllAsync(pageRequest);
        }

        public async Task<PagedResult<GraphConfigEntity>> GetGraphConfigsByTypeAsync(Core.Enums.GraphType type, PageRequest pageRequest)
        {
            return await _graphConfigRepository.GetByTypeAsync(type, pageRequest);
        }

        public async Task<GraphConfigEntity> CreateGraphConfigAsync(CreateGraphConfigRequest request)
        {
            ActionObject? actionObject = null;

            if (request.ParentFolderId.HasValue)
            {
                var folder = await _actionObjectRepository.GetByIdAsync(request.ParentFolderId.Value);
                if (folder == null || folder.ObjectType != ObjectType.Folder)
                    throw new ArgumentException("Folder not found.");

                actionObject = new ActionObject
                {
                    Name = request.Name,
                    ObjectType = ObjectType.Graph,
                    ParentObjectId = request.ParentFolderId.Value,
                    CreatedAt = DateTime.UtcNow
                };
                await _actionObjectRepository.AddAsync(actionObject);
            }

            var graphConfig = new GraphConfigEntity
            {
                Name = request.Name,
                Type = (GraphType)request.Type,
                View = request.View,
                Data = request.Data,
                Meta = request.Meta,
                IsActive = request.IsActive,
                ActionObjectId = actionObject?.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _graphConfigRepository.AddAsync(graphConfig);
            return graphConfig;
        }

        public async Task UpdateGraphConfigAsync(Guid id, UpdateGraphConfigRequest request)
        {
            var graphConfig = await _graphConfigRepository.GetByIdAsync(id);
            if (graphConfig == null)
                throw new KeyNotFoundException($"GraphConfig with id {id} not found");

            graphConfig.Name = request.Name;
            graphConfig.Type = (GraphType)request.Type;
            graphConfig.View = request.View;
            graphConfig.Data = request.Data;
            graphConfig.Meta = request.Meta;
            graphConfig.IsActive = request.IsActive;
            graphConfig.UpdatedAt = DateTime.UtcNow;

            // Handle folder move
            if (request.ParentFolderId.HasValue)
            {
                if (graphConfig.ActionObjectId.HasValue)
                {
                    // Move existing ActionObject to new folder
                    var ao = await _actionObjectRepository.GetByIdAsync(graphConfig.ActionObjectId.Value);
                    if (ao != null)
                    {
                        ao.Name = request.Name;
                        ao.ParentObjectId = request.ParentFolderId.Value;
                        ao.UpdatedAt = DateTime.UtcNow;
                        await _actionObjectRepository.UpdateAsync(ao);
                    }
                }
                else
                {
                    // Create new ActionObject in folder
                    var ao = new ActionObject
                    {
                        Name = request.Name,
                        ObjectType = ObjectType.Graph,
                        ParentObjectId = request.ParentFolderId.Value,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _actionObjectRepository.AddAsync(ao);
                    graphConfig.ActionObjectId = ao.Id;
                }
            }

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

        public async Task<PagedResult<GraphDataDefinitionEntity>> GetAllGraphDataDefinitionsAsync(PageRequest pageRequest)
        {
            return await _graphDataDefinitionRepository.GetAllAsync(pageRequest);
        }

        public async Task<GraphDataDefinitionEntity> CreateGraphDataDefinitionAsync(CreateGraphDataDefinitionRequest request)
        {
            var graphDataDefinition = new GraphDataDefinitionEntity
            {
                GraphConfigId = request.GraphConfigId,
                Source = request.Source,
                SeriesCalculations = request.SeriesCalculations,
                GlobalFilter = request.GlobalFilter,
                SortRules = request.SortRules,
                RowLimit = request.RowLimit,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _graphDataDefinitionRepository.AddAsync(graphDataDefinition);
            return graphDataDefinition;
        }

        public async Task UpdateGraphDataDefinitionAsync(Guid id, UpdateGraphDataDefinitionRequest request)
        {
            var graphDataDefinition = await _graphDataDefinitionRepository.GetByIdAsync(id);
            if (graphDataDefinition == null)
                throw new KeyNotFoundException($"GraphDataDefinition with id {id} not found");

            graphDataDefinition.Source = request.Source;
            graphDataDefinition.SeriesCalculations = request.SeriesCalculations;
            graphDataDefinition.GlobalFilter = request.GlobalFilter;
            graphDataDefinition.SortRules = request.SortRules;
            graphDataDefinition.RowLimit = request.RowLimit;
            graphDataDefinition.IsActive = request.IsActive;
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