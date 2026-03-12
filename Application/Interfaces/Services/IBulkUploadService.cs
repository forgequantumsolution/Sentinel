using Application.DTOs;

namespace Application.Interfaces.Services
{
    /// <summary>
    /// Service interface for handling bulk upload operations.
    /// </summary>
    public interface IBulkUploadService
    {
        /// <summary>
        /// Creates a bulk upload job and queues it for background processing.
        /// </summary>
        /// <param name="dto">The bulk create DTO containing forms to create.</param>
        /// <param name="userId">The ID of the user initiating the upload.</param>
        /// <returns>Response containing job ID and status.</returns>
        Task<BulkUploadJobResponseDto> CreateBulkUploadJobAsync(BulkCreateDynamicFormDto dto, Guid? userId);

        /// <summary>
        /// Gets the status of a bulk upload job.
        /// </summary>
        /// <param name="jobId">The job ID to check.</param>
        /// <returns>Job status DTO or null if not found.</returns>
        Task<BulkUploadJobStatusDto?> GetJobStatusAsync(Guid jobId);
    }
}
