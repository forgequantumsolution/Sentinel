using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    /// <summary>
    /// Stores a user's partially-filled form data so they can continue later.
    /// One draft per user per form — identified by (FormId, CreatedById).
    /// Drafts are separate from submissions and never appear in submission lists.
    /// </summary>
    public class DynamicFormDraft : TenantEntity
    {
        [Required]
        public Guid FormId { get; set; }

        [ForeignKey("FormId")]
        public virtual DynamicForm Form { get; set; }

        /// <summary>
        /// Partial form data as JSON. May be incomplete (not all required fields present).
        /// </summary>
        [Required]
        public string DataJson { get; set; }
    }
}
