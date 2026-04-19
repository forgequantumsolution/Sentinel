using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Persistence;
using Application.DTOs;
using Core.Entities;
using Core.Enums;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentRepository _repository;
        private readonly IUserGroupRepository _userGroupRepository;
        private readonly IDynamicGroupingRuleRepository _ruleRepository;

        public DepartmentsController(
            IDepartmentRepository repository,
            IUserGroupRepository userGroupRepository,
            IDynamicGroupingRuleRepository ruleRepository)
        {
            _repository = repository;
            _userGroupRepository = userGroupRepository;
            _ruleRepository = ruleRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _repository.GetAllAsync();
            var dtos = items.Select(i => new DepartmentDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Code = i.Code,
                ParentDepartmentId = i.ParentDepartmentId,
                IsActive = i.IsActive
            });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound();

            var dto = new DepartmentDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Code = item.Code,
                ParentDepartmentId = item.ParentDepartmentId,
                IsActive = item.IsActive
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DepartmentDto dto)
        {
            var dept = new Department
            {
                Name = dto.Name,
                Description = dto.Description,
                Code = dto.Code,
                ParentDepartmentId = dto.ParentDepartmentId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(dept);
            dto.Id = dept.Id;

            // Auto-create department-based UserGroup + DynamicGroupingRule
            var group = new UserGroup
            {
                Name = $"{dept.Name} Department",
                Description = $"Auto-generated group for department: {dept.Name}",
                Type = GroupType.Department,
                DepartmentId = dept.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _userGroupRepository.AddAsync(group);

            var rule = new DynamicGroupingRule
            {
                Name = $"{dept.Name} Department Rule",
                Description = $"Auto-assign users in {dept.Name} department",
                Field = "User.DepartmentId",
                Operator = RuleOperator.Equals,
                Value = dept.Id.ToString(),
                IsDynamicValue = false,
                IsHidden = true,
                RuleType = RuleType.Simple,
                UserGroupId = group.Id,
                AutoAssign = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _ruleRepository.AddAsync(rule);

            return CreatedAtAction(nameof(GetById), new { id = dept.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] DepartmentDto dto)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound();

            item.Name = dto.Name;
            item.Description = dto.Description;
            item.Code = dto.Code;
            item.ParentDepartmentId = dto.ParentDepartmentId;
            item.IsActive = dto.IsActive;
            item.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(item);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}
