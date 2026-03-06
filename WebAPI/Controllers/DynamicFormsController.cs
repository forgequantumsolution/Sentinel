using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Analytics_BE.Application.DTOs;
using Analytics_BE.Application.Interfaces.Persistence;
using Analytics_BE.Core.Entities;
using System.Security.Claims;
using Application.Common.Pagination;

namespace Analytics_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DynamicFormsController : ControllerBase
    {
        private readonly IDynamicFormRepository _formRepository;
        private readonly IDynamicFormSubmissionRepository _submissionRepository;

        public DynamicFormsController(
            IDynamicFormRepository formRepository,
            IDynamicFormSubmissionRepository submissionRepository)
        {
            _formRepository = formRepository;
            _submissionRepository = submissionRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PageRequest pageRequest)
        {
            var pagedForms = await _formRepository.GetAllAsync(pageRequest);
            var dtos = pagedForms.Items.Select(f => new DynamicFormDto
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                ConfigJson = f.ConfigJson,
                IsActive = f.IsActive,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            });

            var result = new PagedResult<DynamicFormDto>
            {
                Items = dtos,
                TotalCount = pagedForms.TotalCount,
                Page = pagedForms.Page,
                PageSize = pagedForms.PageSize
            };

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var form = await _formRepository.GetByIdAsync(id);
            if (form == null || form.IsDeleted) return NotFound();

            var dto = new DynamicFormDto
            {
                Id = form.Id,
                Name = form.Name,
                Description = form.Description,
                ConfigJson = form.ConfigJson,
                IsActive = form.IsActive,
                CreatedAt = form.CreatedAt,
                UpdatedAt = form.UpdatedAt
            };
            return Ok(dto);
        }

        [HttpPost]
        [Authorize(Roles = "sys-admin")]
        public async Task<IActionResult> Create([FromBody] CreateDynamicFormDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = null;
            if (Guid.TryParse(userIdClaim, out var parsedId))
            {
                userId = parsedId;
            }

            var form = new DynamicForm
            {
                Name = dto.Name,
                Description = dto.Description,
                ConfigJson = dto.ConfigJson,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId
            };

            await _formRepository.AddAsync(form);

            var resultDto = new DynamicFormDto
            {
                Id = form.Id,
                Name = form.Name,
                Description = form.Description,
                ConfigJson = form.ConfigJson,
                IsActive = form.IsActive,
                CreatedAt = form.CreatedAt
            };

            return CreatedAtAction(nameof(GetById), new { id = form.Id }, resultDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "sys-admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateDynamicFormDto dto)
        {
            var form = await _formRepository.GetByIdAsync(id);
            if (form == null || form.IsDeleted) return NotFound();

            form.Name = dto.Name;
            form.Description = dto.Description;
            form.ConfigJson = dto.ConfigJson;
            form.IsActive = dto.IsActive;
            form.UpdatedAt = DateTime.UtcNow;

            await _formRepository.UpdateAsync(form);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "sys-admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var form = await _formRepository.GetByIdAsync(id);
            if (form == null || form.IsDeleted) return NotFound();

            await _formRepository.DeleteAsync(id);

            return NoContent();
        }

        [HttpPost("{id}/submissions")]
        public async Task<IActionResult> SubmitForm(Guid id, [FromBody] CreateDynamicFormSubmissionDto dto)
        {
            var form = await _formRepository.GetByIdAsync(id);
            if (form == null || form.IsDeleted || !form.IsActive) 
                return BadRequest("Form is not available.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = null;
            if (Guid.TryParse(userIdClaim, out var parsedId))
            {
                userId = parsedId;
            }

            var submission = new DynamicFormSubmission
            {
                FormId = id,
                DataJson = dto.DataJson,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId
            };

            await _submissionRepository.AddAsync(submission);

            var resultDto = new DynamicFormSubmissionDto
            {
                Id = submission.Id,
                FormId = submission.FormId,
                DataJson = submission.DataJson,
                CreatedAt = submission.CreatedAt,
                CreatedById = submission.CreatedById
            };

            return Ok(resultDto);
        }

        [HttpGet("{id}/submissions")]
        [Authorize(Roles = "sys-admin")]
        public async Task<IActionResult> GetFormSubmissions(Guid id, [FromQuery] PageRequest pageRequest)
        {
            var pagedSubmissions = await _submissionRepository.GetByFormIdAsync(id, pageRequest);

            var dtos = pagedSubmissions.Items.Select(s => new DynamicFormSubmissionDto
            {
                Id = s.Id,
                FormId = s.FormId,
                DataJson = s.DataJson,
                CreatedAt = s.CreatedAt,
                CreatedById = s.CreatedById
            });

            var result = new PagedResult<DynamicFormSubmissionDto>
            {
                Items = dtos,
                TotalCount = pagedSubmissions.TotalCount,
                Page = pagedSubmissions.Page,
                PageSize = pagedSubmissions.PageSize
            };

            return Ok(result);
        }
    }
}
