using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Application.DTOs;
using Core.Entities;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleRepository _repository;
        private readonly IAutoGroupProvisioningService _provisioning;

        public RolesController(IRoleRepository repository, IAutoGroupProvisioningService provisioning)
        {
            _repository = repository;
            _provisioning = provisioning;
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
            var role = new Role
            {
                Name = dto.Name,
                Description = dto.Description,
                IsDefault = dto.IsDefault,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _provisioning.CreateRoleWithGroupAsync(role);
            dto.Id = role.Id;

            return CreatedAtAction(nameof(GetById), new { id = role.Id }, dto);
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
