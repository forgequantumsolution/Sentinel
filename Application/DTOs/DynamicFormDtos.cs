using System;

namespace Application.DTOs
{
    public class DynamicFormDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ConfigJson { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateDynamicFormFieldDefinitionDto
    {
        public string ColumnName { get; set; }
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public bool IsRequired { get; set; }
        public string? ValidationRules { get; set; }
    }

    public class CreateDynamicFormDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ConfigJson { get; set; }
        public bool IsActive { get; set; } = true;
        public System.Collections.Generic.List<CreateDynamicFormFieldDefinitionDto>? FieldDefinitions { get; set; }
    }

    public class DynamicFormSubmissionDto
    {
        public Guid Id { get; set; }
        public Guid FormId { get; set; }
        public string DataJson { get; set; }
        public Guid? CreatedById { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateDynamicFormSubmissionDto
    {
        public string DataJson { get; set; }
    }

    public class UpdateDynamicFormSubmissionDto
    {
        public string DataJson { get; set; }
    }

    /// <summary>
    /// DTO for bulk creating dynamic forms.
    /// </summary>
    public class BulkCreateDynamicFormDto
    {
        /// <summary>
        /// List of forms to create in bulk.
        /// </summary>
        public System.Collections.Generic.List<CreateDynamicFormDto> Forms { get; set; } = new();
    }

    /// <summary>
    /// Response DTO for bulk upload job initiation.
    /// </summary>
    public class BulkUploadJobResponseDto
    {
        public Guid JobId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int TotalItems { get; set; }
    }

    /// <summary>
    /// DTO for bulk upload job status.
    /// </summary>
    public class BulkUploadJobStatusDto
    {
        public Guid JobId { get; set; }
        public string JobType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int TotalItems { get; set; }
        public int ProcessedItems { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public string? ErrorDetails { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}
