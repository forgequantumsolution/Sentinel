using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class DynamicGroupingRule : BaseDynamicRule<DynamicGroupingRule>
    {
        public bool AutoAssign { get; set; } = true;

        [ForeignKey("ParentRuleId")]
        public override DynamicGroupingRule? ParentRule { get; set; }

        public override ICollection<DynamicGroupingRule> ChildRules { get; set; } = [];

        public bool UserMatchesRule(User user)
        {
            if (!IsActive) return false;
            return Evaluate(user, user);
        }
    }
}
