using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Analytics_BE.Core.Entities
{
    public class DynamicForm : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Store form fields configuration in JSON format.
        /// e.g. [{ "type": "text", "name": "firstName", "label": "First Name" }]
        /// </summary>
        [Required]
        public string ConfigJson { get; set; }
        
        // Navigation properties
        public virtual ICollection<DynamicFormSubmission> Submissions { get; set; } = new List<DynamicFormSubmission>();
        public virtual ICollection<DynamicFormRecord> Records { get; set; } = new List<DynamicFormRecord>();
        public virtual ICollection<DynamicFormFieldDefinition> FieldDefinitions { get; set; } = new List<DynamicFormFieldDefinition>();
    }
}
