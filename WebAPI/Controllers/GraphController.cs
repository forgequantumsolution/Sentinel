using System;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require auth for graph data
    public class GraphController : ControllerBase
    {
        private readonly IGraphService _graphService;

        public GraphController(IGraphService graphService)
        {
            _graphService = graphService;
        }

        [HttpPost("data")]
        public async Task<IActionResult> GetGraphData([FromBody] GraphConfigRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TableName) || string.IsNullOrWhiteSpace(request.GroupByColumn))
            {
                return BadRequest("Invalid graph configuration. TableName and GroupByColumn are required.");
            }

            try
            {
                var graphData = await _graphService.GetGraphDataAsync(request);
                return Ok(graphData);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // In production, log error and return a generic message to prevent leaking DB structure
                return StatusCode(500, "Error generating graph data: " + ex.Message);
            }
        }
    }
}
