using Analytics_BE.Core.Entities;
using Analytics_BE.Infrastructure.Persistence;
using Analytics_BE.Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Analytics_BE.Infrastructure.Extensions
{
    public static class UserGroupExtension
    {
        public static async Task<bool> UserBelongsToGroupAsync(this UserGroup userGroup, User user)
        {
            // First check if user is already explicitly in the collection
            if (userGroup.Users?.Any(u => u.Id == user.Id) == true)
                return true;

            // Then evaluate dynamic grouping rules
            return userGroup.DynamicGroupingRules?.Any(rule => rule.IsActive && rule.UserMatchesRule(user)) == true;
        }

        public static async Task CreateDynamicGroupingRuleAsync(
            this Department department,
            Guid userGroupId,
            bool autoAssign = true)
        {
            var context = Provider.Get<AppDbContext>();

            context.DynamicGroupingRules.Add(new DynamicGroupingRule
            {
                Id = Guid.NewGuid(),
                Name = $"{department.Name}-Dept-Auto-Assign",
                Description = $"Automatically assign users from {department.Name} department",
                Field = "User.Department.Name",
                Operator = RuleOperator.Equals,
                Value = department.Name,
                AutoAssign = autoAssign,
                UserGroupId = userGroupId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = department.CreatedById
            });

            await context.SaveChangesAsync();
        }

        public static DynamicGroupingRule CreateDynamicGroupingRule(
            this Role role,
            Guid userGroupId,
            Guid? createdById,
            bool autoAssign = true)
        {
            return new DynamicGroupingRule
            {
                Id = Guid.NewGuid(),
                Name = $"{role.Name}-Role-Auto-Assign",
                Description = $"Automatically assign users with {role.Name} role",
                Field = "User.Role.Name",
                Operator = RuleOperator.Equals,
                Value = role.Name,
                AutoAssign = autoAssign,
                UserGroupId = userGroupId,
                CreatedAt = DateTime.UtcNow,
                CreatedById = createdById
            };
        }

        public static UserGroup CreateRepresentativeGroup(this Department department)
        {
            return new UserGroup
            {
                Id = Guid.NewGuid(),
                Name = $"{department.Name} Department Group",
                Description = $"Group representing {department.Name} department",
                Type = GroupType.Department,
                DepartmentId = department.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedById = department.CreatedById
            };
        }
    }
}
