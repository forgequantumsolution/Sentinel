using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    /// <summary>
    /// Represents a granular permission/action that can be performed on a feature.
    /// e.g., "Create", "Read", "Update", "Delete", "Export", "Approve"
    /// </summary>
    public class AppPermission : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty; // e.g., "Create", "Read", "Update", "Delete"

        [MaxLength(50)]
        public string? Code { get; set; } // e.g., "CREATE", "READ", "UPDATE", "DELETE"

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation
        public virtual ICollection<FeaturePermissionAssignment> Assignments { get; set; } = new List<FeaturePermissionAssignment>();
    }
}
