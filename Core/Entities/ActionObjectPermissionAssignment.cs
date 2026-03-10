using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;

namespace Core.Entities
{
    /// <summary>
    /// Maps an ActionObject + Permission to an Assignee (Organization or User).
    /// 
    /// Rules:
    /// - When AssigneeType = Organization: grants the actionObject-permission to the entire org.
    ///   This defines the "ceiling" — what the org is licensed/allowed to use.
    /// - When AssigneeType = User: grants the actionObject-permission to a specific user.
    ///   This can ONLY exist if the user's organization already has the same
    ///   ActionObject + Permission assigned (enforced at the service layer).
    /// </summary>
    public class ActionObjectPermissionAssignment : TenantEntity
    {
        // ── What ──
        [Required]
        public Guid ActionObjectId { get; set; }

        [ForeignKey("ActionObjectId")]
        public virtual ActionObject ActionObject { get; set; } = null!;

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