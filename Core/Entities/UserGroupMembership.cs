using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    /// <summary>
    /// Read-only model mapped to the vw_UserGroupMemberships SQL VIEW.
    /// The view evaluates DynamicGroupingRules in real-time.
    /// </summary>
    public class UserGroupMembership
    {
        public Guid UserId { get; set; }
        public Guid UserGroupId { get; set; }
        public Guid? RuleId { get; set; }
        public Guid? OrganizationId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("UserGroupId")]
        public virtual UserGroup? UserGroup { get; set; }
    }
}
