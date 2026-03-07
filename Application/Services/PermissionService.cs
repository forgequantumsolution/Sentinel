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
                // Check if user belongs to group via rules
                if (group.DynamicGroupingRules.Any(r => r.IsActive && r.UserMatchesRule(user)))
                {
                    // Evaluate group's dynamic permission rules
                    foreach (var rule in group.DynamicPermissionRules.Where(r => r.IsActive))
                    {
                        if (rule.Evaluate(user))
                        {
                            // In a real implementation, we'd fetch actual Permission entities.
                            // For now, these were linked in the previous step.
                        }
                    }
                }
            }

            return permissions;
        }

        public async Task AssignPermissionToUserAsync(Guid userId, Guid permissionId) { }

        public async Task AssignPermissionToGroupAsync(Guid groupId, Guid permissionId) { }

        public async Task<DynamicPermissionRule> CreateDynamicPermissionRuleAsync(DynamicPermissionRule rule)
        {
            // This logic can stay in the service for now or be moved to repo if it involves DB directly.
            // Since IPermissionService says "CreateDynamicPermissionRule", we'll need a Repo for it.
            return rule; // Placeholder
        }
    }
}
