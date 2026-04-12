using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Interfaces.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DynamicGroupObjectPermissionsController : ControllerBase
    {
        private readonly IDynamicGroupObjectPermissionService _ruleService;
        private readonly IRuleFieldService _ruleFieldService;

        public DynamicGroupObjectPermissionsController(IDynamicGroupObjectPermissionService ruleService, IRuleFieldService ruleFieldService)
        {
            _ruleService = ruleService;
            _ruleFieldService = ruleFieldService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var rules = await _ruleService.GetAllAsync();
            return Ok(rules);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var rule = await _ruleService.GetByIdAsync(id);
            if (rule == null) return NotFound();
            return Ok(rule);
        }

        [HttpGet("user-group/{userGroupId}")]
        public async Task<IActionResult> GetByUserGroupId(Guid userGroupId)
        {
            var rules = await _ruleService.GetByUserGroupIdAsync(userGroupId);
            return Ok(rules);
        }

        [HttpGet("action-object/{actionObjectId}")]
        public async Task<IActionResult> GetByActionObjectId(Guid actionObjectId)
        {
            var rules = await _ruleService.GetByActionObjectIdAsync(actionObjectId);
            return Ok(rules);
        }

        [HttpGet("permission/{permissionId}")]
        public async Task<IActionResult> GetByPermissionId(Guid permissionId)
        {
            var rules = await _ruleService.GetByPermissionIdAsync(permissionId);
            return Ok(rules);
        }

        [HttpGet("action-object/{actionObjectId}/permission/{permissionId}")]
        public async Task<IActionResult> GetByActionObjectAndPermission(Guid actionObjectId, Guid permissionId)
        {
            var rules = await _ruleService.GetByActionObjectAndPermissionAsync(actionObjectId, permissionId);
            return Ok(rules);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDynamicGroupObjectPermissionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var rule = await _ruleService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = rule.Id }, rule);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDynamicGroupObjectPermissionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var rule = await _ruleService.UpdateAsync(id, request);
                return Ok(rule);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!await _ruleService.ExistsAsync(id))
                return NotFound();

            await _ruleService.DeleteAsync(id);
            return NoContent();
        }
    }
}