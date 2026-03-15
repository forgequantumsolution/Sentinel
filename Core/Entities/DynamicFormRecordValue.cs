using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    /// <summary>
    /// EAV (Entity-Attribute-Value) storage for dynamic form submission data.
    /// Each row stores a single field value for a submission.
    /// </summary>
    public class DynamicFormRecordValue : TenantEntity
    {
        [Required]
        public Guid FormId { get; set; }

        [ForeignKey("FormId")]
        public virtual DynamicForm Form { get; set; } = null!;

        [Required]
        public Guid SubmissionId { get; set; }

        [ForeignKey("SubmissionId")]
        public virtual DynamicFormSubmission Submission { get; set; } = null!;

        [Required]
        public Guid FieldDefinitionId { get; set; }

        [ForeignKey("FieldDefinitionId")]
        public virtual DynamicFormFieldDefinition FieldDefinition { get; set; } = null!;

        /// <summary>
        /// The field value stored as a string. Type interpretation is based on FieldDefinition.FieldType.
        /// </summary>
        public string? Value { get; set; }
    }
}
