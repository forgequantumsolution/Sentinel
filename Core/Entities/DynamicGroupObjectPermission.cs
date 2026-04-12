using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class DynamicGroupObjectPermission : TenantEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public Guid UserGroupId { get; set; }

        [ForeignKey("UserGroupId")]
        public virtual UserGroup? UserGroup { get; set; }

        public bool IsAllowed { get; set; } = true;
        public int Priority { get; set; } = 0;

        // Multiple ActionObject+Permissions sets this rule grants
        public virtual ICollection<ActionObjectPermissionSet> ActionObjectPermissionSets { get; set; } = [];
    }
}
