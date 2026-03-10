using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;

namespace Core.Entities
{
    /// <summary>
    /// Represents any object on which permissions can be applied.
    /// This can be a feature, URL, file, API endpoint, UI component, or any other resource.
    /// </summary>
    public class ActionObject : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Code { get; set; } // e.g., "DYNAMIC_FORMS", "/api/users", "report.pdf"

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Type of the action object
        /// </summary>
        [Required]
        public ObjectType ObjectType { get; set; } = ObjectType.Feature;

        /// <summary>
        /// Optional parent object for hierarchical structures.
        /// e.g., "Analytics Dashboard" > "Chart Builder" or "/api" > "/api/users"
        /// </summary>
        public Guid? ParentObjectId { get; set; }
        
        [ForeignKey("ParentObjectId")]
        public virtual ActionObject? ParentObject { get; set; }
        
        public virtual ICollection<ActionObject> ChildObjects { get; set; } = new List<ActionObject>();

        // Navigation
        public virtual ICollection<ActionObjectPermissionAssignment> Assignments { get; set; } = new List<ActionObjectPermissionAssignment>();
    }
}
