using System;

namespace Application.DTOs
{
    public class DepartmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Code { get; set; }
        public Guid? ParentDepartmentId { get; set; }
        public bool IsActive { get; set; }
    }

    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }

    public class JobTitleDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? DepartmentId { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Type { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? RoleId { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid RoleId { get; set; }
        public string? RoleName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? JobTitleId { get; set; }
        public string? JobTitle { get; set; }
        public string? Location { get; set; }
        public string? EmployeeId { get; set; }
        public int Status { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateUserRequest : UserDto
    {
        public string Password { get; set; } = string.Empty;
    }

    // ── RBAC DTOs ──
    public class FeatureDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public Guid? ParentFeatureId { get; set; }
        public bool IsActive { get; set; }
    }

    public class AppPermissionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class FeaturePermissionDto
    {
        public Guid FeatureId { get; set; }
        public FeatureDto? Feature { get; set; }
        public Guid PermissionId { get; set; }
        public AppPermissionDto? Permission { get; set; }
    }

    public class UserFeaturesPermissionsDto
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<FeaturePermissionDto> FeaturesPermissions { get; set; } = new();
    }

    // ── Graph DTOs ──
    public class GraphConfigDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Type { get; set; } // GraphType enum
        public Core.Models.GraphViewConfig View { get; set; } = new();
        public Core.Models.GraphDataConfig Data { get; set; } = new();
        public Dictionary<string, object>? Meta { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedById { get; set; }
        public Guid? OrganizationId { get; set; }
    }

    public class GraphDataDefinitionDto
    {
        public Guid Id { get; set; }
        public Guid GraphConfigId { get; set; }
        public Core.Models.DataSourceDefinition Source { get; set; } = new();
        public List<Core.Models.SeriesCalculation> SeriesCalculations { get; set; } = new();
        public Core.Models.FilterGroup? GlobalFilter { get; set; }
        public List<Core.Models.SortRule>? SortRules { get; set; }
        public int? RowLimit { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedById { get; set; }
        public Guid? OrganizationId { get; set; }
    }

    public class CreateGraphConfigRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Type { get; set; }
        public Core.Models.GraphViewConfig View { get; set; } = new();
        public Core.Models.GraphDataConfig Data { get; set; } = new();
        public Dictionary<string, object>? Meta { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? ParentFolderId { get; set; }
    }

    public class UpdateGraphConfigRequest
    {
        public string Name { get; set; } = string.Empty;
        public int Type { get; set; }
        public Core.Models.GraphViewConfig View { get; set; } = new();
        public Core.Models.GraphDataConfig Data { get; set; } = new();
        public Dictionary<string, object>? Meta { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? ParentFolderId { get; set; }
    }

    public class CreateGraphDataDefinitionRequest
    {
        public Guid GraphConfigId { get; set; }
        public Core.Models.DataSourceDefinition Source { get; set; } = new();
        public List<Core.Models.SeriesCalculation> SeriesCalculations { get; set; } = new();
        public Core.Models.FilterGroup? GlobalFilter { get; set; }
        public List<Core.Models.SortRule>? SortRules { get; set; }
        public int? RowLimit { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateGraphDataDefinitionRequest
    {
        public Core.Models.DataSourceDefinition Source { get; set; } = new();
        public List<Core.Models.SeriesCalculation> SeriesCalculations { get; set; } = new();
        public Core.Models.FilterGroup? GlobalFilter { get; set; }
        public List<Core.Models.SortRule>? SortRules { get; set; }
        public int? RowLimit { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
