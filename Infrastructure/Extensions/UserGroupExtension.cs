using System.Linq.Expressions;
using Core.Entities;
using Infrastructure.Persistence;
using Core.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions
{
    public static class UserGroupExtension
    {
        public static async Task<bool> UserBelongsToGroupAsync(this UserGroup userGroup, User user, AppDbContext context)
        {
            // First check if user is already explicitly in the collection
            if (userGroup.Users?.Any(u => u.Id == user.Id) == true)
                return true;

            // Evaluate dynamic grouping rules via expression in DB
            var rules = userGroup.DynamicGroupingRules?
                .Where(r => r.IsActive && r.ParentRuleId == null)
                .ToList();

            if (rules == null || rules.Count == 0)
                return false;

            foreach (var rule in rules)
            {
                var ruleExpr = rule.ToExpression();
                var param = ruleExpr.Parameters[0];
                var idCheck = Expression.Equal(
                    Expression.Property(param, nameof(User.Id)),
                    Expression.Constant(user.Id));
                var combined = Expression.AndAlso(idCheck, ruleExpr.Body);
                var predicate = Expression.Lambda<Func<User, bool>>(combined, param);

                if (await context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Department)
                    .Include(u => u.JobTitle)
                    .AnyAsync(predicate))
                {
                    return true;
                }
            }

            return false;
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
