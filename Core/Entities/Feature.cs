using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    /// <summary>
    /// Represents a feature/module in the application.
    /// e.g., "Dynamic Forms", "Analytics Dashboard", "User Management"
    /// </summary>
    public class Feature : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Code { get; set; } // e.g., "DYNAMIC_FORMS", "ANALYTICS_DASHBOARD"

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// Optional parent feature for hierarchical feature trees.
        /// e.g., "Analytics Dashboard" > "Chart Builder"
        /// </summary>
        public Guid? ParentFeatureId { get; set; }
        public virtual Feature? ParentFeature { get; set; }
        public virtual ICollection<Feature> ChildFeatures { get; set; } = new List<Feature>();

        // Navigation
        public virtual ICollection<FeaturePermissionAssignment> Assignments { get; set; } = new List<FeaturePermissionAssignment>();
    }
}
