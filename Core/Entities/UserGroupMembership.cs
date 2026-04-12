using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    /// <summary>
    /// Read-only model mapped to the vw_UserGroupMemberships SQL VIEW.
    /// Resolves: User → GroupingRule → UserGroup → DynamicGroupObjectPermission → ActionObject + Permission
    /// </summary>
    public class UserGroupMembership
    {
        public Guid UserId { get; set; }
        public Guid UserGroupId { get; set; }
        public Guid? RuleId { get; set; }
        public Guid? ActionObjectId { get; set; }
        public Guid? PermissionId { get; set; }
        public Guid? OrganizationId { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("UserGroupId")]
        public virtual UserGroup? UserGroup { get; set; }

        [ForeignKey("ActionObjectId")]
        public virtual ActionObject? ActionObject { get; set; }

        [ForeignKey("PermissionId")]
        public virtual AppPermission? Permission { get; set; }
    }
}
