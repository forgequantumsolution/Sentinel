using Microsoft.AspNetCore.Mvc;
using Application.Common.Pagination;
using Application.DTOs;
using Application.Interfaces.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DynamicGroupingRulesController : ControllerBase
    {
        private readonly IDynamicGroupingRuleService _ruleService;
        private readonly IRuleFieldService _ruleFieldService;

        public DynamicGroupingRulesController(IDynamicGroupingRuleService ruleService, IRuleFieldService ruleFieldService)
        {
            _ruleService = ruleService;
            _ruleFieldService = ruleFieldService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PageRequest pageRequest)
        {
            var rules = await _ruleService.GetAllAsync(pageRequest);
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
        public async Task<IActionResult> GetByUserGroupId(Guid userGroupId, [FromQuery] PageRequest pageRequest)
        {
            var rules = await _ruleService.GetByUserGroupIdAsync(userGroupId, pageRequest);
            return Ok(rules);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDynamicGroupingRuleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var rule = await _ruleService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = rule.Id }, rule);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDynamicGroupingRuleRequest request)
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

        [HttpGet("{ruleId}/user/{userId}/matches")]
        public async Task<IActionResult> UserMatchesRule(Guid ruleId, Guid userId)
        {
            if (!await _ruleService.ExistsAsync(ruleId))
                return NotFound();

            var matches = await _ruleService.UserMatchesRuleAsync(ruleId, userId);
            return Ok(new { matches });
        }

        /// <summary>
        /// Returns all user properties available for rule definitions, with applicable operators
        /// </summary>
        [HttpGet("rule-fields")]
        public IActionResult GetRuleFields()
        {
            return Ok(_ruleFieldService.GetRuleFields());
        }
    }
}