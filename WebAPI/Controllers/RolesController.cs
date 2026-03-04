using Microsoft.AspNetCore.Mvc;
using Analytics_BE.Application.Interfaces.Persistence;
using Analytics_BE.Application.DTOs;
using Analytics_BE.Core.Entities;

namespace Analytics_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _repository;

        public RolesController(IRoleRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _repository.GetAllAsync();
            var dtos = items.Select(i => new RoleDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                IsDefault = i.IsDefault,
                IsActive = i.IsActive
            });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound();

            var dto = new RoleDto
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                IsDefault = item.IsDefault,
                IsActive = item.IsActive
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RoleDto dto)
        {
            var item = new Role
            {
                Name = dto.Name,
                Description = dto.Description,
                IsDefault = dto.IsDefault,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(item);
            dto.Id = item.Id;
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] RoleDto dto)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound();

            item.Name = dto.Name;
            item.Description = dto.Description;
            item.IsDefault = dto.IsDefault;
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
