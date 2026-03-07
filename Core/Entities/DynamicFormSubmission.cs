using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class DynamicFormSubmission : TenantEntity
    {
        [Required]
        public Guid FormId { get; set; }

        [ForeignKey("FormId")]
        public virtual DynamicForm Form { get; set; }

        /// <summary>
        /// Store actual submitted data in JSON format.
        /// </summary>
        [Required]
        public string DataJson { get; set; }
    }
}
