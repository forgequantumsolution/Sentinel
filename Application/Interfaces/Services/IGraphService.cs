using Core.Entities;
using Core.Models;
using Application.DTOs;
using Application.Common.Pagination;

namespace Application.Interfaces.Services
{
    public interface IGraphService
    {
        // GraphConfig operations
        Task<GraphConfigEntity?> GetGraphConfigByIdAsync(Guid id);
        Task<GraphConfigEntity?> GetGraphConfigByNameAsync(string name);
        Task<PagedResult<GraphConfigEntity>> GetAllGraphConfigsAsync(PageRequest pageRequest);
        Task<PagedResult<GraphConfigEntity>> GetGraphConfigsByTypeAsync(Core.Enums.GraphType type, PageRequest pageRequest);
        Task<GraphConfigEntity> CreateGraphConfigAsync(GraphConfigDto graphConfigDto);
        Task UpdateGraphConfigAsync(Guid id, GraphConfigDto graphConfigDto);
        Task DeleteGraphConfigAsync(Guid id);

        // GraphDataDefinition operations
        Task<GraphDataDefinitionEntity?> GetGraphDataDefinitionByIdAsync(Guid id);
        Task<GraphDataDefinitionEntity?> GetGraphDataDefinitionByGraphConfigIdAsync(Guid graphConfigId);
        Task<PagedResult<GraphDataDefinitionEntity>> GetAllGraphDataDefinitionsAsync(PageRequest pageRequest);
        Task<GraphDataDefinitionEntity> CreateGraphDataDefinitionAsync(GraphDataDefinitionDto graphDataDefinitionDto);
        Task UpdateGraphDataDefinitionAsync(Guid id, GraphDataDefinitionDto graphDataDefinitionDto);
        Task DeleteGraphDataDefinitionAsync(Guid id);
        Task DeleteGraphDataDefinitionsByGraphConfigIdAsync(Guid graphConfigId);

        // Combined operations
        Task<GraphPayload> GetGraphPayloadAsync(Guid graphConfigId);
        Task<GraphPayload> ExecuteGraphAsync(Guid graphConfigId, Dictionary<string, object>? parameters = null);
    }
}