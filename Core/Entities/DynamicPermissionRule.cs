using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class DynamicPermissionRule : BaseDynamicRule<DynamicPermissionRule>
    {
        public Guid? FolderPermissionId { get; set; }
        public Guid? WorkflowPermissionId { get; set; }
        public Guid? RequestPermissionId { get; set; }

        public bool IsAllowed { get; set; } = true;
        public int Priority { get; set; } = 0;
        public bool IsInherited { get; set; } = false;
        public bool IsInheritable { get; set; } = true;

        [ForeignKey("ParentRuleId")]
        public override DynamicPermissionRule? ParentRule { get; set; }
        public override ICollection<DynamicPermissionRule> ChildRules { get; set; } = [];
    }
}
