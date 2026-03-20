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
        Task<GraphConfigEntity> CreateGraphConfigAsync(CreateGraphConfigRequest request);
        Task UpdateGraphConfigAsync(Guid id, UpdateGraphConfigRequest request);
        Task DeleteGraphConfigAsync(Guid id);

        // GraphDataDefinition operations
        Task<GraphDataDefinitionEntity?> GetGraphDataDefinitionByIdAsync(Guid id);
        Task<GraphDataDefinitionEntity?> GetGraphDataDefinitionByGraphConfigIdAsync(Guid graphConfigId);
        Task<PagedResult<GraphDataDefinitionEntity>> GetAllGraphDataDefinitionsAsync(PageRequest pageRequest);
        Task<GraphDataDefinitionEntity> CreateGraphDataDefinitionAsync(CreateGraphDataDefinitionRequest request);
        Task UpdateGraphDataDefinitionAsync(Guid id, UpdateGraphDataDefinitionRequest request);
        Task DeleteGraphDataDefinitionAsync(Guid id);
        Task DeleteGraphDataDefinitionsByGraphConfigIdAsync(Guid graphConfigId);

        // UI component operations (non-graph components stored in the same table)
        Task<PagedResult<GraphConfigEntity>> GetUiComponentsByTypeAsync(Core.Enums.UiComponentType componentType, PageRequest pageRequest);

        // Combined operations
        Task<GraphPayload> GetGraphPayloadAsync(Guid graphConfigId);
        Task<GraphPayload> ExecuteGraphAsync(Guid graphConfigId, GraphExecuteRequest? request = null);
    }
}