using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;

namespace Core.Entities
{
    public class UserGroup : TenantEntity
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public GroupType Type { get; set; } = GroupType.Group;
        
        // Type-specific foreign keys
        public Guid? DepartmentId { get; set; }
        public Guid? RoleId { get; set; }
        
        [ForeignKey("DepartmentId")]
        public virtual Department? Department { get; set; }
        
        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }

        public virtual ICollection<User> Users { get; set; } = [];
        public virtual ICollection<DynamicGroupingRule> DynamicGroupingRules { get; set; } = [];
        public virtual ICollection<DynamicGroupObjectPermission> DynamicGroupObjectPermissions { get; set; } = [];
    }
}
