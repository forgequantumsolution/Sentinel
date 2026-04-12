using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    /// <summary>
    /// Defines one ActionObject and the list of permissions granted on it.
    /// Linked to a DynamicGroupObjectPermission via DynamicGroupObjectPermissionId.
    /// </summary>
    public class ActionObjectPermissionSet : BaseEntity
    {
        [Required]
        public Guid DynamicGroupObjectPermissionId { get; set; }

        [ForeignKey("DynamicGroupObjectPermissionId")]
        public virtual DynamicGroupObjectPermission? DynamicGroupObjectPermission { get; set; }

        [Required]
        public Guid ActionObjectId { get; set; }

        [ForeignKey("ActionObjectId")]
        public virtual ActionObject? ActionObject { get; set; }

        public virtual ICollection<ActionObjectPermissionSetItem> Permissions { get; set; } = [];
    }

    public class ActionObjectPermissionSetItem : BaseEntity
    {
        [Required]
        public Guid ActionObjectPermissionSetId { get; set; }

        [ForeignKey("ActionObjectPermissionSetId")]
        public virtual ActionObjectPermissionSet? ActionObjectPermissionSet { get; set; }

        [Required]
        public Guid PermissionId { get; set; }

        [ForeignKey("PermissionId")]
        public virtual AppPermission? Permission { get; set; }
    }
}
