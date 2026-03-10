using System;
using Core.Enums;

namespace Application.DTOs
{
    public class DynamicGroupingRuleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public RuleOperator Operator { get; set; }
        public string Value { get; set; } = string.Empty;
        public bool IsDynamicValue { get; set; } = false;
        public RuleType RuleType { get; set; } = RuleType.Simple;
        public Guid? ParentRuleId { get; set; }
        public Guid? UserGroupId { get; set; }
        public bool AutoAssign { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedById { get; set; }
        public Guid? OrganizationId { get; set; }
        
        // Navigation properties for display
        public string? UserGroupName { get; set; }
        public string? ParentRuleName { get; set; }
        public List<DynamicGroupingRuleDto> ChildRules { get; set; } = new();
    }

    public class CreateDynamicGroupingRuleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public RuleOperator Operator { get; set; }
        public string Value { get; set; } = string.Empty;
        public bool IsDynamicValue { get; set; } = false;
        public RuleType RuleType { get; set; } = RuleType.Simple;
        public Guid? ParentRuleId { get; set; }
        public Guid? UserGroupId { get; set; }
        public bool AutoAssign { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public List<CreateDynamicGroupingRuleRequest>? ChildRules { get; set; }
    }

    public class UpdateDynamicGroupingRuleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public RuleOperator Operator { get; set; }
        public string Value { get; set; } = string.Empty;
        public bool IsDynamicValue { get; set; } = false;
        public RuleType RuleType { get; set; } = RuleType.Simple;
        public Guid? ParentRuleId { get; set; }
        public Guid? UserGroupId { get; set; }
        public bool AutoAssign { get; set; } = true;
        public bool IsActive { get; set; } = true;
    }

    public class DynamicPermissionRuleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public RuleOperator Operator { get; set; }
        public string Value { get; set; } = string.Empty;
        public bool IsDynamicValue { get; set; } = false;
        public RuleType RuleType { get; set; } = RuleType.Simple;
        public Guid? ParentRuleId { get; set; }
        public Guid? UserGroupId { get; set; }
        public Guid? ActionObjectId { get; set; }
        public Guid? PermissionId { get; set; }
        public bool IsAllowed { get; set; } = true;
        public int Priority { get; set; } = 0;
        public bool IsInherited { get; set; } = false;
        public bool IsInheritable { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? CreatedById { get; set; }
        public Guid? OrganizationId { get; set; }
        
        // Navigation properties for display
        public string? UserGroupName { get; set; }
        public string? ParentRuleName { get; set; }
        public string? ActionObjectName { get; set; }
        public string? PermissionName { get; set; }
        public List<DynamicPermissionRuleDto> ChildRules { get; set; } = new();
    }

    public class CreateDynamicPermissionRuleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public RuleOperator Operator { get; set; }
        public string Value { get; set; } = string.Empty;
        public bool IsDynamicValue { get; set; } = false;
        public RuleType RuleType { get; set; } = RuleType.Simple;
        public Guid? ParentRuleId { get; set; }
        public Guid? UserGroupId { get; set; }
        public Guid? ActionObjectId { get; set; }
        public Guid? PermissionId { get; set; }
        public bool IsAllowed { get; set; } = true;
        public int Priority { get; set; } = 0;
        public bool IsInherited { get; set; } = false;
        public bool IsInheritable { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public List<CreateDynamicPermissionRuleRequest>? ChildRules { get; set; }
    }

    public class UpdateDynamicPermissionRuleRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Field { get; set; } = string.Empty;
        public RuleOperator Operator { get; set; }
        public string Value { get; set; } = string.Empty;
        public bool IsDynamicValue { get; set; } = false;
        public RuleType RuleType { get; set; } = RuleType.Simple;
        public Guid? ParentRuleId { get; set; }
        public Guid? UserGroupId { get; set; }
        public Guid? ActionObjectId { get; set; }
        public Guid? PermissionId { get; set; }
        public bool IsAllowed { get; set; } = true;
        public int Priority { get; set; } = 0;
        public bool IsInherited { get; set; } = false;
        public bool IsInheritable { get; set; } = true;
        public bool IsActive { get; set; } = true;
    }
}