using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Core.Entities;
using System.Security.Claims;
using Application.Common.Pagination;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DynamicFormsController : ControllerBase
    {
        private readonly IDynamicFormRepository _formRepository;
        private readonly IDynamicFormSubmissionRepository _submissionRepository;
        private readonly IBulkUploadService _bulkUploadService;
        private readonly Infrastructure.Persistence.AppDbContext _context;

        public DynamicFormsController(
            IDynamicFormRepository formRepository,
            IDynamicFormSubmissionRepository submissionRepository,
            IBulkUploadService bulkUploadService,
            Infrastructure.Persistence.AppDbContext context)
        {
            _formRepository = formRepository;
            _submissionRepository = submissionRepository;
            _bulkUploadService = bulkUploadService;
            _context = context;
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

            if (dto.FieldDefinitions == null || dto.FieldDefinitions.Count == 0)
                return BadRequest("FieldDefinitions are required and cannot be empty.");

            var form = new DynamicForm
            {
                Name = dto.Name,
                Description = dto.Description,
                ConfigJson = dto.ConfigJson,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId,
                FieldDefinitions = dto.FieldDefinitions.Select(fd => new DynamicFormFieldDefinition
                {
                    ColumnName = fd.ColumnName,
                    FieldName = fd.FieldName,
                    FieldType = fd.FieldType,
                    IsRequired = fd.IsRequired,
                    ValidationRules = fd.ValidationRules
                }).ToList()
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

        /// <summary>
        /// Bulk upload dynamic forms. The forms are processed in a background job to avoid blocking the thread.
        /// </summary>
        /// <param name="dto">The bulk create DTO containing an array of forms to create.</param>
        /// <returns>Job ID and status for tracking the bulk upload progress.</returns>
        [HttpPost("bulk")]
        [Authorize(Roles = "sys-admin")]
        public async Task<IActionResult> BulkCreate([FromBody] BulkCreateDynamicFormDto dto)
        {
            if (dto.Forms == null || dto.Forms.Count == 0)
            {
                return BadRequest("No forms provided for bulk upload.");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = null;
            if (Guid.TryParse(userIdClaim, out var parsedId))
            {
                userId = parsedId;
            }

            try
            {
                var response = await _bulkUploadService.CreateBulkUploadJobAsync(dto, userId);
                return Accepted(response);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get the status of a bulk upload job.
        /// </summary>
        /// <param name="jobId">The job ID returned from the bulk upload endpoint.</param>
        /// <returns>Job status including progress and any errors.</returns>
        [HttpGet("bulk/jobs/{jobId}")]
        [Authorize(Roles = "sys-admin")]
        public async Task<IActionResult> GetBulkUploadJobStatus(Guid jobId)
        {
            var status = await _bulkUploadService.GetJobStatusAsync(jobId);
            if (status == null)
            {
                return NotFound("Job not found.");
            }

            return Ok(status);
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
            await _context.SaveChangesAsync();

            // Dynamically populate columns in DynamicFormRecord
            try
            {
                var dataDict = ParseDataJson(dto.DataJson);

                if (dataDict != null && form.FieldDefinitions != null)
                {
                    var record = new DynamicFormRecord 
                    { 
                        FormId = id, 
                        SubmissionId = submission.Id,
                        CreatedById = userId, 
                        CreatedAt = DateTime.UtcNow 
                    };

                    MapFieldsToRecord(record, form.FieldDefinitions, dataDict);

                    _context.DynamicFormRecords.Add(record);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                // Handle JSON deserialization or reflection errors silently or log them
            }

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

        [HttpPut("{formId}/submissions/{submissionId}")]
        public async Task<IActionResult> UpdateSubmission(Guid formId, Guid submissionId, [FromBody] UpdateDynamicFormSubmissionDto dto)
        {
            var form = await _formRepository.GetByIdAsync(formId);
            if (form == null || form.IsDeleted)
                return NotFound("Form not found.");

            var submission = await _submissionRepository.GetByIdAsync(submissionId);
            if (submission == null || submission.IsDeleted)
                return NotFound("Submission not found.");

            if (submission.FormId != formId)
                return BadRequest("Submission does not belong to this form.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = null;
            if (Guid.TryParse(userIdClaim, out var parsedId))
            {
                userId = parsedId;
            }

            submission.DataJson = dto.DataJson;
            submission.UpdatedAt = DateTime.UtcNow;

            await _submissionRepository.UpdateAsync(submission);

            // Update corresponding DynamicFormRecord if exists
            try
            {
                var dataDict = ParseDataJson(dto.DataJson);

                if (dataDict != null && form.FieldDefinitions != null)
                {
                    var record = await _context.DynamicFormRecords
                        .FirstOrDefaultAsync(r => r.FormId == formId && submissionId == r.SubmissionId);

                    if (record != null)
                    {
                        MapFieldsToRecord(record, form.FieldDefinitions, dataDict);

                        record.UpdatedAt = DateTime.UtcNow;
                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception)
            {
                // Handle JSON deserialization or reflection errors silently or log them
            }

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

        [HttpDelete("{formId}/submissions/{submissionId}")]
        [Authorize(Roles = "sys-admin")]
        public async Task<IActionResult> DeleteSubmission(Guid formId, Guid submissionId)
        {
            var form = await _formRepository.GetByIdAsync(formId);
            if (form == null || form.IsDeleted)
                return NotFound("Form not found.");

            var submission = await _submissionRepository.GetByIdAsync(submissionId);
            if (submission == null || submission.IsDeleted)
                return NotFound("Submission not found.");

            if (submission.FormId != formId)
                return BadRequest("Submission does not belong to this form.");

            await _submissionRepository.DeleteAsync(submissionId);

            // Soft delete corresponding DynamicFormRecord if exists
            try
            {
                var record = await _context.DynamicFormRecords
                    .FirstOrDefaultAsync(r => r.FormId == formId && submissionId == r.SubmissionId);

                if (record != null)
                {
                    record.IsDeleted = true;
                    record.DeletedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                // Handle errors silently or log them
            }

            return NoContent();
        }

        #region Private Helper Methods

        /// <summary>
        /// Parses the JSON data string into a dictionary for field mapping.
        /// </summary>
        private static Dictionary<string, System.Text.Json.JsonElement>? ParseDataJson(string dataJson)
        {
            var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.Text.Json.JsonElement>>(dataJson, options);
        }

        /// <summary>
        /// Maps field values from the data dictionary to the DynamicFormRecord entity based on field definitions.
        /// </summary>
        private static void MapFieldsToRecord(
            DynamicFormRecord record, 
            IEnumerable<DynamicFormFieldDefinition> fieldDefinitions, 
            Dictionary<string, System.Text.Json.JsonElement> dataDict)
        {
            foreach (var fieldDef in fieldDefinitions)
            {
                // Match either FieldName or ColumnName from incoming JSON
                System.Text.Json.JsonElement jsonElement;
                bool found = dataDict.TryGetValue(fieldDef.FieldName, out jsonElement) || 
                             dataDict.TryGetValue(fieldDef.ColumnName, out jsonElement);

                if (found)
                {
                    var stringValue = jsonElement.ValueKind == System.Text.Json.JsonValueKind.String 
                        ? jsonElement.GetString() 
                        : jsonElement.GetRawText();

                    var prop = typeof(DynamicFormRecord).GetProperty(fieldDef.ColumnName);
                    if (prop != null && prop.CanWrite)
                    {
                        prop.SetValue(record, stringValue);
                    }
                }
            }
        }

        #endregion
    }
}
