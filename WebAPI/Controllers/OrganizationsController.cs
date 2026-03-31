using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Persistence;
using Application.DTOs;
using Core.Entities;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationRepository _repository;

        public OrganizationsController(IOrganizationRepository repository)
        {
            _repository = repository;
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
            var item = new Organization
            {
                Name = dto.Name,
                Code = dto.Code,
                Description = dto.Description,
                ParentOrganizationId = dto.ParentOrganizationId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(item);
            dto.Id = item.Id;
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, dto);
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
