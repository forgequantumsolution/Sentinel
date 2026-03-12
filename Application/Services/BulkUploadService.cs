using System.Text.Json;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Core.Entities;

namespace Application.Services
{
    /// <summary>
    /// Service for handling bulk upload job creation and status tracking.
    /// </summary>
    public class BulkUploadService : IBulkUploadService
    {
        private readonly IBulkUploadJobRepository _jobRepository;

        public BulkUploadService(IBulkUploadJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<BulkUploadJobResponseDto> CreateBulkUploadJobAsync(BulkCreateDynamicFormDto dto, Guid? userId)
        {
            if (dto.Forms == null || dto.Forms.Count == 0)
            {
                throw new ArgumentException("No forms provided for bulk upload.");
            }

            var job = new BulkUploadJob
            {
                JobType = "DynamicForm",
                Status = BulkUploadJobStatus.Pending,
                TotalItems = dto.Forms.Count,
                ProcessedItems = 0,
                SuccessCount = 0,
                FailureCount = 0,
                PayloadJson = JsonSerializer.Serialize(dto),
                CreatedAt = DateTime.UtcNow,
                CreatedById = userId
            };

            await _jobRepository.AddAsync(job);

            return new BulkUploadJobResponseDto
            {
                JobId = job.Id,
                Status = job.Status.ToString(),
                Message = "Bulk upload job has been queued for processing.",
                TotalItems = job.TotalItems
            };
        }

        public async Task<BulkUploadJobStatusDto?> GetJobStatusAsync(Guid jobId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                return null;
            }

            return new BulkUploadJobStatusDto
            {
                JobId = job.Id,
                JobType = job.JobType,
                Status = job.Status.ToString(),
                TotalItems = job.TotalItems,
                ProcessedItems = job.ProcessedItems,
                SuccessCount = job.SuccessCount,
                FailureCount = job.FailureCount,
                ErrorDetails = job.ErrorDetails,
                CreatedAt = job.CreatedAt,
                StartedAt = job.StartedAt,
                CompletedAt = job.CompletedAt
            };
        }
    }
}
