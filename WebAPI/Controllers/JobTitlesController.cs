using Microsoft.AspNetCore.Mvc;
using Application.Common.Pagination;
using Application.Interfaces.Persistence;
using Application.DTOs;
using Core.Entities;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobTitlesController : ControllerBase
    {
        private readonly IJobTitleRepository _repository;

        public JobTitlesController(IJobTitleRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PageRequest pageRequest)
        {
            var paged = await _repository.GetAllAsync(pageRequest);
            var dtos = paged.Items.Select(i => new JobTitleDto
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                DepartmentId = i.DepartmentId,
                IsActive = i.IsActive
            });

            return Ok(new PagedResult<JobTitleDto>
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

            var dto = new JobTitleDto
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                DepartmentId = item.DepartmentId,
                IsActive = item.IsActive
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JobTitleDto dto)
        {
            var item = new JobTitle
            {
                Title = dto.Title,
                Description = dto.Description,
                DepartmentId = dto.DepartmentId,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };
            await _repository.AddAsync(item);
            dto.Id = item.Id;
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] JobTitleDto dto)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound();

            item.Title = dto.Title;
            item.Description = dto.Description;
            item.DepartmentId = dto.DepartmentId;
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
