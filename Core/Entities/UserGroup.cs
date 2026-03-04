using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Analytics_BE.Core.Enums;

namespace Analytics_BE.Core.Entities
{
    public class UserGroup : BaseEntity
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
        public virtual ICollection<DynamicPermissionRule> DynamicPermissionRules { get; set; } = [];
    }
}
