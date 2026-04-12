using System;

namespace Application.DTOs
{
    public class OrganizationDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public Guid? ParentOrganizationId { get; set; }
        public bool IsActive { get; set; }
    }

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
        public Guid? OrganizationId { get; set; }
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

        /// <summary>
        /// Null = graph (see GraphType enum for Type).
        /// Set = UI component: 1=KpiCard, 2=Table, 3=Metric, 4=DataGrid, 99=Custom.
        /// </summary>
        public int? ComponentType { get; set; }

        /// <summary>Only meaningful when ComponentType is null.</summary>
        public int Type { get; set; }

        /// <summary>
        /// Visual / presentation config as raw JSON.
        /// For graphs see Core.Models.GraphViewConfig for the expected shape.
        /// For KPI cards see Core.Models.KpiCardConfig.
        /// </summary>
        public System.Text.Json.JsonElement? View { get; set; }

        /// <summary>
        /// Data source config as raw JSON.
        /// For graphs see Core.Models.GraphDataConfig / DataSourceDefinition.
        /// </summary>
        public System.Text.Json.JsonElement? Data { get; set; }

        public Dictionary<string, object>? Meta { get; set; }
        public Dictionary<string, object>? FiltersParams { get; set; }
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

        /// <summary>Null for graphs. Set to UiComponentType value for UI components.</summary>
        public int? ComponentType { get; set; }

        /// <summary>Only meaningful when ComponentType is null.</summary>
        public int Type { get; set; }

        /// <summary>Visual / presentation config — any JSON object.</summary>
        public System.Text.Json.JsonElement? View { get; set; }

        /// <summary>Data source config — any JSON object.</summary>
        public System.Text.Json.JsonElement? Data { get; set; }

        public Dictionary<string, object>? Meta { get; set; }
        public Dictionary<string, object>? FiltersParams { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? ParentFolderId { get; set; }
    }

    public class UpdateGraphConfigRequest
    {
        public string Name { get; set; } = string.Empty;

        /// <summary>Null for graphs. Set to UiComponentType value for UI components.</summary>
        public int? ComponentType { get; set; }

        /// <summary>Only meaningful when ComponentType is null.</summary>
        public int Type { get; set; }

        /// <summary>Visual / presentation config — any JSON object.</summary>
        public System.Text.Json.JsonElement? View { get; set; }

        /// <summary>Data source config — any JSON object.</summary>
        public System.Text.Json.JsonElement? Data { get; set; }

        public Dictionary<string, object>? Meta { get; set; }
        public Dictionary<string, object>? FiltersParams { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? ParentFolderId { get; set; }
    }

    public class CreateGraphDataDefinitionRequest
    {
        public Guid GraphConfigId { get; set; }
        public Core.Models.DataSourceDefinition Source { get; set; } = new();
        // TODO: will add once functionality is developed
        // public List<Core.Models.SeriesCalculation> SeriesCalculations { get; set; } = new();
        // public Core.Models.FilterGroup? GlobalFilter { get; set; }
        // public List<Core.Models.SortRule>? SortRules { get; set; }
        // public int? RowLimit { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateGraphDataDefinitionRequest
    {
        public Core.Models.DataSourceDefinition Source { get; set; } = new();
        // TODO: will add once functionality is developed
        // public List<Core.Models.SeriesCalculation> SeriesCalculations { get; set; } = new();
        // public Core.Models.FilterGroup? GlobalFilter { get; set; }
        // public List<Core.Models.SortRule>? SortRules { get; set; }
        // public int? RowLimit { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Request body sent by the frontend when executing a graph.
    /// Allows runtime filter/sort overrides on top of the saved data definition.
    /// </summary>
    public class GraphExecuteRequest
    {
        /// <summary>Additional filters applied at runtime (merged with saved GlobalFilter via AND).</summary>
        public Core.Models.FilterGroup? Filters { get; set; }

        /// <summary>Override sort rules for this execution (replaces saved SortRules when provided).</summary>
        public List<Core.Models.SortRule>? SortRules { get; set; }

        /// <summary>Override row limit for this execution.</summary>
        public int? RowLimit { get; set; }

        /// <summary>Named parameters for SQL / DynamicForm queries (e.g. @startDate).</summary>
        public Dictionary<string, object>? Parameters { get; set; }
    }

    public class RuleFieldOperatorDto
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class RuleFieldDto
    {
        public string Field { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public List<RuleFieldOperatorDto> Operators { get; set; } = [];
    }

    public class ActionObjectPermissionSetDto
    {
        public Guid Id { get; set; }
        public Guid ActionObjectId { get; set; }
        public string? ActionObjectName { get; set; }
        public List<ActionObjectPermissionSetItemDto> Permissions { get; set; } = [];
    }

    public class ActionObjectPermissionSetItemDto
    {
        public Guid Id { get; set; }
        public Guid PermissionId { get; set; }
        public string? PermissionName { get; set; }
    }

    public class CreateActionObjectPermissionSetRequest
    {
        public Guid ActionObjectId { get; set; }
        public List<Guid> PermissionIds { get; set; } = [];
    }

    public class UserGroupMembershipDto
    {
        public Guid UserId { get; set; }
        public Guid UserGroupId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? GroupName { get; set; }
        public Guid? RuleId { get; set; }
        public Guid? ActionObjectId { get; set; }
        public string? ActionObjectName { get; set; }
        public Guid? PermissionId { get; set; }
        public string? PermissionName { get; set; }
    }
}
