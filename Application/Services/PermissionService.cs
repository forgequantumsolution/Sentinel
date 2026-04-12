using System.Linq.Expressions;
using Core.Entities;
using Core.Enums;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserGroupRepository _userGroupRepository;

        public PermissionService(IUserRepository userRepository, IUserGroupRepository userGroupRepository)
        {
            _userRepository = userRepository;
            _userGroupRepository = userGroupRepository;
        }

        public async Task<bool> HasPermissionAsync(Guid userId, string action, Guid? resourceId = null, PermissionType resourceType = PermissionType.System)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            // Admin bypass
            if (user.Role?.Name == "admin" || user.Role?.Name == "sys-admin") return true;

            var effectivePermissions = await GetEffectivePermissionsAsync(userId, resourceId, resourceType);
            return effectivePermissions.Any(p => p.Allowed.Contains(action));
        }

        public async Task<List<Permission>> GetEffectivePermissionsAsync(Guid userId, Guid? resourceId = null, PermissionType resourceType = PermissionType.System)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return [];

            var permissions = new List<Permission>();

            // Dynamic Group Permissions
            var allGroupsWithRules = await _userGroupRepository.GetAllWithRulesAsync();

            foreach (var group in allGroupsWithRules)
            {
                // Check if user belongs to group via expression-based DB query
                var groupingRules = group.DynamicGroupingRules.Where(r => r.IsActive && r.ParentRuleId == null).ToList();
                bool belongsToGroup = false;

                foreach (var rule in groupingRules)
                {
                    var ruleExpr = rule.ToExpression();
                    var param = ruleExpr.Parameters[0];
                    var idCheck = Expression.Equal(
                        Expression.Property(param, nameof(User.Id)),
                        Expression.Constant(userId));
                    var combined = Expression.AndAlso(idCheck, ruleExpr.Body);
                    var predicate = Expression.Lambda<Func<User, bool>>(combined, param);

                    if (await _userRepository.AnyMatchAsync(predicate))
                    {
                        belongsToGroup = true;
                        break;
                    }
                }

                if (belongsToGroup)
                {
                    // DynamicGroupObjectPermissions now directly assign permissions to groups
                    // — no user evaluation needed, the group membership is the gate.
                }
            }

            return permissions;
        }

        public async Task AssignPermissionToUserAsync(Guid userId, Guid permissionId) { }

        public async Task AssignPermissionToGroupAsync(Guid groupId, Guid permissionId) { }

        public async Task<DynamicGroupObjectPermission> CreateDynamicGroupObjectPermissionAsync(DynamicGroupObjectPermission rule)
        {
            // This logic can stay in the service for now or be moved to repo if it involves DB directly.
            // Since IPermissionService says "CreateDynamicGroupObjectPermission", we'll need a Repo for it.
            return rule; // Placeholder
        }
    }
}
