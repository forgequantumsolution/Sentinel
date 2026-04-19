using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Persistence;
using Application.DTOs;
using Core.Entities;
using Core.Enums;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationRepository _repository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserGroupRepository _userGroupRepository;
        private readonly IDynamicGroupingRuleRepository _ruleRepository;

        public OrganizationsController(
            IOrganizationRepository repository,
            IRoleRepository roleRepository,
            IUserGroupRepository userGroupRepository,
            IDynamicGroupingRuleRepository ruleRepository)
        {
            _repository = repository;
            _roleRepository = roleRepository;
            _userGroupRepository = userGroupRepository;
            _ruleRepository = ruleRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _repository.GetAllAsync();
            var dtos = items.Select(i => new OrganizationDto
            {
                Id = i.Id,
                Name = i.Name,
                Code = i.Code,
                Description = i.Description,
                ParentOrganizationId = i.ParentOrganizationId,
                IsActive = i.IsActive
            });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound();

            var dto = new OrganizationDto
            {
                Id = item.Id,
                Name = item.Name,
                Code = item.Code,
                Description = item.Description,
                ParentOrganizationId = item.ParentOrganizationId,
                IsActive = item.IsActive
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrganizationDto dto)
        {
            var org = new Organization
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                ParentOrganizationId = dto.ParentOrganizationId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(org);

            // Auto-create an admin role for the new organization
            await _roleRepository.AddAsync(new Role
            {
                Name = "admin",
                Description = $"Administrator - Full access for {org.Name}",
                IsDefault = false,
                IsActive = true,
                OrganizationId = org.Id,
                CreatedAt = DateTime.UtcNow
            });

            // Auto-create org-based UserGroup + DynamicGroupingRule
            // OrganizationId is set explicitly so the new group/rule belong to the new org,
            // not the current request's tenant.
            var group = new UserGroup
            {
                Name = $"{org.Name} Organization",
                Description = $"Auto-generated group for organization: {org.Name}",
                Type = GroupType.Group,
                OrganizationId = org.Id,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _userGroupRepository.AddAsync(group);

            var rule = new DynamicGroupingRule
            {
                Name = $"{org.Name} Organization Rule",
                Description = $"Auto-assign users in {org.Name} organization",
                Field = "User.OrganizationId",
                Operator = RuleOperator.Equals,
                Value = org.Id.ToString(),
                IsDynamicValue = false,
                IsHidden = true,
                RuleType = RuleType.Simple,
                UserGroupId = group.Id,
                AutoAssign = true,
                IsActive = true,
                OrganizationId = org.Id,
                CreatedAt = DateTime.UtcNow
            };
            await _ruleRepository.AddAsync(rule);

            dto.Id = org.Id;
            return CreatedAtAction(nameof(GetById), new { id = org.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrganizationDto dto)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound();

            item.Name = dto.Name;
            item.Code = dto.Code;
            item.Description = dto.Description;
            item.ParentOrganizationId = dto.ParentOrganizationId;
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
