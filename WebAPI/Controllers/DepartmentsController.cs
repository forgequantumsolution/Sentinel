using Microsoft.AspNetCore.Mvc;
using Application.Common.Pagination;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Application.DTOs;
using Core.Entities;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentRepository _repository;
        private readonly IAutoGroupProvisioningService _provisioning;

        public DepartmentsController(IDepartmentRepository repository, IAutoGroupProvisioningService provisioning)
        {
            _repository = repository;
            _provisioning = provisioning;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PageRequest pageRequest)
        {
            var paged = await _repository.GetAllAsync(pageRequest);
            var dtos = paged.Items.Select(i => new DepartmentDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Code = i.Code,
                ParentDepartmentId = i.ParentDepartmentId,
                IsActive = i.IsActive
            });

            return Ok(new PagedResult<DepartmentDto>
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

            await _provisioning.CreateDepartmentWithGroupAsync(dept);
            dto.Id = dept.Id;

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
