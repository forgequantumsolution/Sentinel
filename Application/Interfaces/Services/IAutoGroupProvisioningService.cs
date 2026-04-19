using Core.Entities;

namespace Application.Interfaces.Services
{
    /// <summary>
    /// Creates a Role / Department / Organization together with its
    /// auto-generated UserGroup and DynamicGroupingRule.
    /// </summary>
    public interface IAutoGroupProvisioningService
    {
        Task<Role> CreateRoleWithGroupAsync(Role role);
        Task<Department> CreateDepartmentWithGroupAsync(Department department);
        Task CreateOrganizationGroupAsync(Organization organization);
    }
}
