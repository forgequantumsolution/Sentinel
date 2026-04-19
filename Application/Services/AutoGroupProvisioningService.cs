using Core.Entities;
using Core.Enums;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class AutoGroupProvisioningService : IAutoGroupProvisioningService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IUserGroupRepository _userGroupRepository;
        private readonly IDynamicGroupingRuleRepository _ruleRepository;

        public AutoGroupProvisioningService(
            IRoleRepository roleRepository,
            IDepartmentRepository departmentRepository,
            IUserGroupRepository userGroupRepository,
            IDynamicGroupingRuleRepository ruleRepository)
        {
            _roleRepository = roleRepository;
            _departmentRepository = departmentRepository;
            _userGroupRepository = userGroupRepository;
            _ruleRepository = ruleRepository;
        }

        public async Task<Role> CreateRoleWithGroupAsync(Role role)
        {
            await _roleRepository.AddAsync(role);
            await CreateGroupAndRuleAsync(
                groupName: $"{role.Name} Role",
                groupDescription: $"Auto-generated group for role: {role.Name}",
                groupType: GroupType.Role,
                organizationId: role.OrganizationId,
                ruleField: "User.RoleId",
                ruleValue: role.Id.ToString(),
                ruleNameSuffix: "Role",
                roleId: role.Id);
            return role;
        }

        public async Task<Department> CreateDepartmentWithGroupAsync(Department department)
        {
            await _departmentRepository.AddAsync(department);
            await CreateGroupAndRuleAsync(
                groupName: $"{department.Name} Department",
                groupDescription: $"Auto-generated group for department: {department.Name}",
                groupType: GroupType.Department,
                organizationId: department.OrganizationId,
                ruleField: "User.DepartmentId",
                ruleValue: department.Id.ToString(),
                ruleNameSuffix: "Department",
                departmentId: department.Id);
            return department;
        }

        public async Task CreateOrganizationGroupAsync(Organization organization)
        {
            await CreateGroupAndRuleAsync(
                groupName: $"{organization.Name} Organization",
                groupDescription: $"Auto-generated group for organization: {organization.Name}",
                groupType: GroupType.Group,
                organizationId: organization.Id,
                ruleField: "User.OrganizationId",
                ruleValue: organization.Id.ToString(),
                ruleNameSuffix: "Organization");
        }

        private async Task CreateGroupAndRuleAsync(
            string groupName,
            string groupDescription,
            GroupType groupType,
            Guid? organizationId,
            string ruleField,
            string ruleValue,
            string ruleNameSuffix,
            Guid? roleId = null,
            Guid? departmentId = null)
        {
            var group = new UserGroup
            {
                Name = groupName,
                Description = groupDescription,
                Type = groupType,
                RoleId = roleId,
                DepartmentId = departmentId,
                OrganizationId = organizationId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _userGroupRepository.AddAsync(group);

            var rule = new DynamicGroupingRule
            {
                Name = $"{groupName.Replace(" " + ruleNameSuffix, "")} {ruleNameSuffix} Rule",
                Description = $"Auto-assign rule for {groupName}",
                Field = ruleField,
                Operator = RuleOperator.Equals,
                Value = ruleValue,
                IsDynamicValue = false,
                IsHidden = true,
                RuleType = RuleType.Simple,
                UserGroupId = group.Id,
                AutoAssign = true,
                IsActive = true,
                OrganizationId = organizationId,
                CreatedAt = DateTime.UtcNow
            };
            await _ruleRepository.AddAsync(rule);
        }
    }
}
