using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;

namespace Core.Entities
{
    /// <summary>
    /// Maps a Feature + Permission to an Assignee (Organization or User).
    /// 
    /// Rules:
    /// - When AssigneeType = Organization: grants the feature-permission to the entire org.
    ///   This defines the "ceiling" — what the org is licensed/allowed to use.
    /// - When AssigneeType = User: grants the feature-permission to a specific user.
    ///   This can ONLY exist if the user's organization already has the same
    ///   Feature + Permission assigned (enforced at the service layer).
    /// </summary>
    public class FeaturePermissionAssignment : TenantEntity
    {
        // ── What ──
        [Required]
        public Guid FeatureId { get; set; }

        [ForeignKey("FeatureId")]
        public virtual Feature Feature { get; set; } = null!;

        [Required]
        public Guid PermissionId { get; set; }

        [ForeignKey("PermissionId")]
        public virtual AppPermission Permission { get; set; } = null!;

        // ── Who ──
        [Required]
        public AssigneeType AssigneeType { get; set; }

        /// <summary>
        /// When AssigneeType = Organization, this is the Organization's Id.
        /// When AssigneeType = User, this is the User's Id.
        /// </summary>
        [Required]
        public Guid AssigneeId { get; set; }
    }
}
