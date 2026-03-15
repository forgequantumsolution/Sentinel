using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Common.Pagination;
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
        public async Task<IActionResult> GetAllGraphConfigs([FromQuery] PageRequest pageRequest)
        {
            var pagedResult = await _graphService.GetAllGraphConfigsAsync(pageRequest);

            return Ok(new PagedResult<GraphConfigDto>
            {
                Items = pagedResult.Items.Select(MapToDto),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            });
        }

        [HttpGet("configs/{id}")]
        public async Task<IActionResult> GetGraphConfigById(Guid id)
        {
            var graphConfig = await _graphService.GetGraphConfigByIdAsync(id);
            if (graphConfig == null) return NotFound();
            return Ok(MapToDto(graphConfig));
        }

        [HttpGet("configs/name/{name}")]
        public async Task<IActionResult> GetGraphConfigByName(string name)
        {
            var graphConfig = await _graphService.GetGraphConfigByNameAsync(name);
            if (graphConfig == null) return NotFound();
            return Ok(MapToDto(graphConfig));
        }

        [HttpGet("configs/type/{type}")]
        public async Task<IActionResult> GetGraphConfigsByType(int type, [FromQuery] PageRequest pageRequest)
        {
            var pagedResult = await _graphService.GetGraphConfigsByTypeAsync((Core.Enums.GraphType)type, pageRequest);

            return Ok(new PagedResult<GraphConfigDto>
            {
                Items = pagedResult.Items.Select(MapToDto),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            });
        }

        [HttpPost("configs")]
        public async Task<IActionResult> CreateGraphConfig([FromBody] CreateGraphConfigRequest request)
        {
            var graphConfig = await _graphService.CreateGraphConfigAsync(request);
            return CreatedAtAction(nameof(GetGraphConfigById), new { id = graphConfig.Id }, MapToDto(graphConfig));
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
        public async Task<IActionResult> GetAllGraphDataDefinitions([FromQuery] PageRequest pageRequest)
        {
            var pagedResult = await _graphService.GetAllGraphDataDefinitionsAsync(pageRequest);

            return Ok(new PagedResult<GraphDataDefinitionDto>
            {
                Items = pagedResult.Items.Select(MapToDataDefDto),
                TotalCount = pagedResult.TotalCount,
                Page = pagedResult.Page,
                PageSize = pagedResult.PageSize
            });
        }

        [HttpGet("data-definitions/{id}")]
        public async Task<IActionResult> GetGraphDataDefinitionById(Guid id)
        {
            var dataDefinition = await _graphService.GetGraphDataDefinitionByIdAsync(id);
            if (dataDefinition == null) return NotFound();
            return Ok(MapToDataDefDto(dataDefinition));
        }

        [HttpGet("configs/{graphConfigId}/data-definition")]
        public async Task<IActionResult> GetGraphDataDefinitionByGraphConfigId(Guid graphConfigId)
        {
            var dataDefinition = await _graphService.GetGraphDataDefinitionByGraphConfigIdAsync(graphConfigId);
            if (dataDefinition == null) return NotFound();
            return Ok(MapToDataDefDto(dataDefinition));
        }

        [HttpPost("data-definitions")]
        public async Task<IActionResult> CreateGraphDataDefinition([FromBody] CreateGraphDataDefinitionRequest request)
        {
            var dataDefinition = await _graphService.CreateGraphDataDefinitionAsync(request);
            return CreatedAtAction(nameof(GetGraphDataDefinitionById), new { id = dataDefinition.Id }, MapToDataDefDto(dataDefinition));
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

        // ── Mapping helpers ──

        private static GraphConfigDto MapToDto(GraphConfigEntity g) => new()
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
        };

        private static GraphDataDefinitionDto MapToDataDefDto(GraphDataDefinitionEntity d) => new()
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
        };
    }
}
