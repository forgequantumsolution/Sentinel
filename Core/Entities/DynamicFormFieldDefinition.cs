using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Analytics_BE.Core.Entities
{
    public class DynamicFormFieldDefinition : BaseEntity
    {
        [Required]
        public Guid FormId { get; set; }

        [ForeignKey("FormId")]
        public virtual DynamicForm Form { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string ColumnName { get; set; } // e.g., "Field1", "Field2", etc.

        [Required]
        [MaxLength(255)]
        public string FieldName { get; set; } // e.g., "First Name", "Age", "Date of Birth"

        [Required]
        [MaxLength(50)]
        public string FieldType { get; set; } // e.g., "String", "Int", "DateTime", "Boolean", "Decimal"

        public bool IsRequired { get; set; } = false;
        
        [MaxLength(500)]
        public string? ValidationRules { get; set; } // Optional: JSON storing specific validation rules (min, max, etc.)
    }
}
