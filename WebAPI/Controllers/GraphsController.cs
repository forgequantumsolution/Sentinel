using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Core.Entities;
using Application.Interfaces.Services;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GraphsController : ControllerBase
    {
        private readonly IGraphService _graphService;

        public GraphsController(IGraphService graphService)
        {
            _graphService = graphService;
        }

        // GraphConfig endpoints

        [HttpGet("configs")]
        public async Task<IActionResult> GetAllGraphConfigs()
        {
            var graphConfigs = await _graphService.GetAllGraphConfigsAsync();
            var dtos = graphConfigs.Select(g => new GraphConfigDto
            {
                Id = g.Id,
                Name = g.Name,
                Type = (int)g.Type,
                View = g.View,
                Data = g.Data,
                Meta = g.Meta,
                IsActive = g.IsActive,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt,
                CreatedById = g.CreatedById,
                OrganizationId = g.OrganizationId
            });
            return Ok(dtos);
        }

        [HttpGet("configs/{id}")]
        public async Task<IActionResult> GetGraphConfigById(Guid id)
        {
            var graphConfig = await _graphService.GetGraphConfigByIdAsync(id);
            if (graphConfig == null) return NotFound();

            var dto = new GraphConfigDto
            {
                Id = graphConfig.Id,
                Name = graphConfig.Name,
                Type = (int)graphConfig.Type,
                View = graphConfig.View,
                Data = graphConfig.Data,
                Meta = graphConfig.Meta,
                IsActive = graphConfig.IsActive,
                CreatedAt = graphConfig.CreatedAt,
                UpdatedAt = graphConfig.UpdatedAt,
                CreatedById = graphConfig.CreatedById,
                OrganizationId = graphConfig.OrganizationId
            };
            return Ok(dto);
        }

        [HttpGet("configs/name/{name}")]
        public async Task<IActionResult> GetGraphConfigByName(string name)
        {
            var graphConfig = await _graphService.GetGraphConfigByNameAsync(name);
            if (graphConfig == null) return NotFound();

            var dto = new GraphConfigDto
            {
                Id = graphConfig.Id,
                Name = graphConfig.Name,
                Type = (int)graphConfig.Type,
                View = graphConfig.View,
                Data = graphConfig.Data,
                Meta = graphConfig.Meta,
                IsActive = graphConfig.IsActive,
                CreatedAt = graphConfig.CreatedAt,
                UpdatedAt = graphConfig.UpdatedAt,
                CreatedById = graphConfig.CreatedById,
                OrganizationId = graphConfig.OrganizationId
            };
            return Ok(dto);
        }

        [HttpGet("configs/type/{type}")]
        public async Task<IActionResult> GetGraphConfigsByType(int type)
        {
            var graphConfigs = await _graphService.GetGraphConfigsByTypeAsync((Core.Enums.GraphType)type);
            var dtos = graphConfigs.Select(g => new GraphConfigDto
            {
                Id = g.Id,
                Name = g.Name,
                Type = (int)g.Type,
                View = g.View,
                Data = g.Data,
                Meta = g.Meta,
                IsActive = g.IsActive,
                CreatedAt = g.CreatedAt,
                UpdatedAt = g.UpdatedAt,
                CreatedById = g.CreatedById,
                OrganizationId = g.OrganizationId
            });
            return Ok(dtos);
        }

        [HttpPost("configs")]
        public async Task<IActionResult> CreateGraphConfig([FromBody] CreateGraphConfigRequest request)
        {
            var graphConfig = await _graphService.CreateGraphConfigAsync(request);
            
            var dto = new GraphConfigDto
            {
                Id = graphConfig.Id,
                Name = graphConfig.Name,
                Type = (int)graphConfig.Type,
                View = graphConfig.View,
                Data = graphConfig.Data,
                Meta = graphConfig.Meta,
                IsActive = graphConfig.IsActive,
                CreatedAt = graphConfig.CreatedAt,
                UpdatedAt = graphConfig.UpdatedAt,
                CreatedById = graphConfig.CreatedById,
                OrganizationId = graphConfig.OrganizationId
            };
            
            return CreatedAtAction(nameof(GetGraphConfigById), new { id = graphConfig.Id }, dto);
        }

        [HttpPut("configs/{id}")]
        public async Task<IActionResult> UpdateGraphConfig(Guid id, [FromBody] GraphConfigDto dto)
        {
            await _graphService.UpdateGraphConfigAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("configs/{id}")]
        public async Task<IActionResult> DeleteGraphConfig(Guid id)
        {
            await _graphService.DeleteGraphConfigAsync(id);
            return NoContent();
        }

        // GraphDataDefinition endpoints

        [HttpGet("data-definitions")]
        public async Task<IActionResult> GetAllGraphDataDefinitions()
        {
            var dataDefinitions = await _graphService.GetAllGraphDataDefinitionsAsync();
            var dtos = dataDefinitions.Select(d => new GraphDataDefinitionDto
            {
                Id = d.Id,
                GraphConfigId = d.GraphConfigId,
                Source = d.Source,
                SeriesCalculations = d.SeriesCalculations,
                GlobalFilter = d.GlobalFilter,
                SortRules = d.SortRules,
                RowLimit = d.RowLimit,
                IsActive = d.IsActive,
                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                CreatedById = d.CreatedById,
                OrganizationId = d.OrganizationId
            });
            return Ok(dtos);
        }

        [HttpGet("data-definitions/{id}")]
        public async Task<IActionResult> GetGraphDataDefinitionById(Guid id)
        {
            var dataDefinition = await _graphService.GetGraphDataDefinitionByIdAsync(id);
            if (dataDefinition == null) return NotFound();

            var dto = new GraphDataDefinitionDto
            {
                Id = dataDefinition.Id,
                GraphConfigId = dataDefinition.GraphConfigId,
                Source = dataDefinition.Source,
                SeriesCalculations = dataDefinition.SeriesCalculations,
                GlobalFilter = dataDefinition.GlobalFilter,
                SortRules = dataDefinition.SortRules,
                RowLimit = dataDefinition.RowLimit,
                IsActive = dataDefinition.IsActive,
                CreatedAt = dataDefinition.CreatedAt,
                UpdatedAt = dataDefinition.UpdatedAt,
                CreatedById = dataDefinition.CreatedById,
                OrganizationId = dataDefinition.OrganizationId
            };
            return Ok(dto);
        }

        [HttpGet("configs/{graphConfigId}/data-definition")]
        public async Task<IActionResult> GetGraphDataDefinitionByGraphConfigId(Guid graphConfigId)
        {
            var dataDefinition = await _graphService.GetGraphDataDefinitionByGraphConfigIdAsync(graphConfigId);
            if (dataDefinition == null) return NotFound();

            var dto = new GraphDataDefinitionDto
            {
                Id = dataDefinition.Id,
                GraphConfigId = dataDefinition.GraphConfigId,
                Source = dataDefinition.Source,
                SeriesCalculations = dataDefinition.SeriesCalculations,
                GlobalFilter = dataDefinition.GlobalFilter,
                SortRules = dataDefinition.SortRules,
                RowLimit = dataDefinition.RowLimit,
                IsActive = dataDefinition.IsActive,
                CreatedAt = dataDefinition.CreatedAt,
                UpdatedAt = dataDefinition.UpdatedAt,
                CreatedById = dataDefinition.CreatedById,
                OrganizationId = dataDefinition.OrganizationId
            };
            return Ok(dto);
        }

        [HttpPost("data-definitions")]
        public async Task<IActionResult> CreateGraphDataDefinition([FromBody] CreateGraphDataDefinitionRequest request)
        {
            var dataDefinition = await _graphService.CreateGraphDataDefinitionAsync(request);
            
            var dto = new GraphDataDefinitionDto
            {
                Id = dataDefinition.Id,
                GraphConfigId = dataDefinition.GraphConfigId,
                Source = dataDefinition.Source,
                SeriesCalculations = dataDefinition.SeriesCalculations,
                GlobalFilter = dataDefinition.GlobalFilter,
                SortRules = dataDefinition.SortRules,
                RowLimit = dataDefinition.RowLimit,
                IsActive = dataDefinition.IsActive,
                CreatedAt = dataDefinition.CreatedAt,
                UpdatedAt = dataDefinition.UpdatedAt,
                CreatedById = dataDefinition.CreatedById,
                OrganizationId = dataDefinition.OrganizationId
            };
            
            return CreatedAtAction(nameof(GetGraphDataDefinitionById), new { id = dataDefinition.Id }, dto);
        }

        [HttpPut("data-definitions/{id}")]
        public async Task<IActionResult> UpdateGraphDataDefinition(Guid id, [FromBody] GraphDataDefinitionDto dto)
        {
            await _graphService.UpdateGraphDataDefinitionAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("data-definitions/{id}")]
        public async Task<IActionResult> DeleteGraphDataDefinition(Guid id)
        {
            await _graphService.DeleteGraphDataDefinitionAsync(id);
            return NoContent();
        }

        // Graph execution endpoints

        [HttpGet("configs/{id}/payload")]
        public async Task<IActionResult> GetGraphPayload(Guid id)
        {
            try
            {
                var payload = await _graphService.GetGraphPayloadAsync(id);
                return Ok(payload);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("configs/{id}/execute")]
        public async Task<IActionResult> ExecuteGraph(Guid id, [FromBody] Dictionary<string, object>? parameters = null)
        {
            try
            {
                var payload = await _graphService.ExecuteGraphAsync(id, parameters);
                return Ok(payload);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}