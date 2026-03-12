using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    /// <summary>
    /// Entity to track bulk upload job status and progress.
    /// </summary>
    public class BulkUploadJob : TenantEntity
    {
        [Required]
        [MaxLength(50)]
        public string JobType { get; set; } = "DynamicForm";

        [Required]
        public BulkUploadJobStatus Status { get; set; } = BulkUploadJobStatus.Pending;

        public int TotalItems { get; set; }

        public int ProcessedItems { get; set; }

        public int SuccessCount { get; set; }

        public int FailureCount { get; set; }

        /// <summary>
        /// JSON containing error details for failed items.
        /// </summary>
        public string? ErrorDetails { get; set; }

        /// <summary>
        /// JSON containing the original request payload for processing.
        /// </summary>
        [Required]
        public string PayloadJson { get; set; } = string.Empty;

        public DateTime? StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }
    }

    public enum BulkUploadJobStatus
    {
        Pending = 0,
        Processing = 1,
        Completed = 2,
        PartiallyCompleted = 3,
        Failed = 4
    }
}
