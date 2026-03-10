using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class DynamicPermissionRule : BaseDynamicRule<DynamicPermissionRule>
    {
        // Reference to the ActionObject on which permission is being applied
        public Guid? ActionObjectId { get; set; }
        
        [ForeignKey("ActionObjectId")]
        public virtual ActionObject? ActionObject { get; set; }
        
        // Reference to the specific permission/action
        public Guid? PermissionId { get; set; }
        
        [ForeignKey("PermissionId")]
        public virtual AppPermission? Permission { get; set; }

        public bool IsAllowed { get; set; } = true;
        public int Priority { get; set; } = 0;
        public bool IsInherited { get; set; } = false;
        public bool IsInheritable { get; set; } = true;

        [ForeignKey("ParentRuleId")]
        public override DynamicPermissionRule? ParentRule { get; set; }
        public override ICollection<DynamicPermissionRule> ChildRules { get; set; } = [];
    }
}
