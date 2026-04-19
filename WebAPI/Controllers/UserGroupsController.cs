using Microsoft.AspNetCore.Mvc;
using Application.Common.Pagination;
using Application.Interfaces.Persistence;
using Application.DTOs;
using Core.Entities;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserGroupsController : ControllerBase
    {
        private readonly IUserGroupRepository _repository;

        public UserGroupsController(IUserGroupRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PageRequest pageRequest)
        {
            var paged = await _repository.GetAllAsync(pageRequest);
            var dtos = paged.Items.Select(i => new UserGroupDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Type = (int)i.Type,
                DepartmentId = i.DepartmentId,
                RoleId = i.RoleId,
                IsActive = i.IsActive
            });

            return Ok(new PagedResult<UserGroupDto>
            {
                Items = dtos,
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound();

            var dto = new UserGroupDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                Type = (int)item.Type,
                DepartmentId = item.DepartmentId,
                RoleId = item.RoleId,
                IsActive = item.IsActive
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserGroupDto dto)
        {
            var item = new UserGroup
            {
                Name = dto.Name,
                Description = dto.Description,
                Type = (Core.Enums.GroupType)dto.Type,
                DepartmentId = dto.DepartmentId,
                RoleId = dto.RoleId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(item);
            dto.Id = item.Id;
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserGroupDto dto)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound();

            item.Name = dto.Name;
            item.Description = dto.Description;
            item.Type = (Core.Enums.GroupType)dto.Type;
            item.DepartmentId = dto.DepartmentId;
            item.RoleId = dto.RoleId;
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
