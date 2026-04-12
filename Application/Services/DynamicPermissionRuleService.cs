using System.Linq.Expressions;
using Core.Entities;
using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class DynamicPermissionRuleService : IDynamicPermissionRuleService
    {
        private readonly IDynamicPermissionRuleRepository _ruleRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITenantContext _tenantContext;

        public DynamicPermissionRuleService(
            IDynamicPermissionRuleRepository ruleRepository,
            IUserRepository userRepository,
            ITenantContext tenantContext)
        {
            _ruleRepository = ruleRepository;
            _userRepository = userRepository;
            _tenantContext = tenantContext;
        }

        public async Task<DynamicPermissionRuleDto?> GetByIdAsync(Guid id)
        {
            var rule = await _ruleRepository.GetByIdAsync(id);
            if (rule == null) return null;

            return MapToDto(rule);
        }

        public async Task<List<DynamicPermissionRuleDto>> GetAllAsync()
        {
            var rules = await _ruleRepository.GetAllAsync();
            return rules.Select(MapToDto).ToList();
        }

        public async Task<List<DynamicPermissionRuleDto>> GetByUserGroupIdAsync(Guid userGroupId)
        {
            var rules = await _ruleRepository.GetByUserGroupIdAsync(userGroupId);
            return rules.Select(MapToDto).ToList();
        }

        public async Task<List<DynamicPermissionRuleDto>> GetByActionObjectIdAsync(Guid actionObjectId)
        {
            var rules = await _ruleRepository.GetByActionObjectIdAsync(actionObjectId);
            return rules.Select(MapToDto).ToList();
        }

        public async Task<List<DynamicPermissionRuleDto>> GetByPermissionIdAsync(Guid permissionId)
        {
            var rules = await _ruleRepository.GetByPermissionIdAsync(permissionId);
            return rules.Select(MapToDto).ToList();
        }

        public async Task<List<DynamicPermissionRuleDto>> GetByActionObjectAndPermissionAsync(Guid actionObjectId, Guid permissionId)
        {
            var rules = await _ruleRepository.GetByActionObjectAndPermissionAsync(actionObjectId, permissionId);
            return rules.Select(MapToDto).ToList();
        }

        public async Task<DynamicPermissionRuleDto> CreateAsync(CreateDynamicPermissionRuleRequest request)
        {
            var rule = new DynamicPermissionRule
            {
                Name = request.Name,
                Description = request.Description,
                Field = request.Field,
                Operator = request.Operator,
                Value = request.Value,
                IsDynamicValue = request.IsDynamicValue,
                RuleType = request.RuleType,
                ParentRuleId = request.ParentRuleId,
                UserGroupId = request.UserGroupId,
                ActionObjectId = request.ActionObjectId,
                PermissionId = request.PermissionId,
                IsAllowed = request.IsAllowed,
                Priority = request.Priority,
                IsInherited = request.IsInherited,
                IsInheritable = request.IsInheritable,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            // Handle child rules if provided
            if (request.ChildRules != null && request.ChildRules.Any())
            {
                var childRules = new List<DynamicPermissionRule>();
                foreach (var childRequest in request.ChildRules)
                {
                    var childRule = new DynamicPermissionRule
                    {
                        Name = childRequest.Name,
                        Description = childRequest.Description,
                        Field = childRequest.Field,
                        Operator = childRequest.Operator,
                        Value = childRequest.Value,
                        IsDynamicValue = childRequest.IsDynamicValue,
                        RuleType = childRequest.RuleType,
                        ParentRuleId = rule.Id, // Will be set after rule is saved
                        UserGroupId = childRequest.UserGroupId,
                        ActionObjectId = childRequest.ActionObjectId,
                        PermissionId = childRequest.PermissionId,
                        IsAllowed = childRequest.IsAllowed,
                        Priority = childRequest.Priority,
                        IsInherited = childRequest.IsInherited,
                        IsInheritable = childRequest.IsInheritable,
                        IsActive = childRequest.IsActive,
                        CreatedAt = DateTime.UtcNow
                    };
                    childRules.Add(childRule);
                }
                rule.ChildRules = childRules;
            }

            await _ruleRepository.AddAsync(rule);
            
            // Update child rules with parent ID
            if (rule.ChildRules != null && rule.ChildRules.Any())
            {
                foreach (var child in rule.ChildRules)
                {
                    child.ParentRuleId = rule.Id;
                    await _ruleRepository.UpdateAsync(child);
                }
            }

            return MapToDto(rule);
        }

        public async Task<DynamicPermissionRuleDto> UpdateAsync(Guid id, UpdateDynamicPermissionRuleRequest request)
        {
            var rule = await _ruleRepository.GetByIdAsync(id);
            if (rule == null)
                throw new KeyNotFoundException($"Dynamic permission rule with ID {id} not found");

            rule.Name = request.Name;
            rule.Description = request.Description;
            rule.Field = request.Field;
            rule.Operator = request.Operator;
            rule.Value = request.Value;
            rule.IsDynamicValue = request.IsDynamicValue;
            rule.RuleType = request.RuleType;
            rule.ParentRuleId = request.ParentRuleId;
            rule.UserGroupId = request.UserGroupId;
            rule.ActionObjectId = request.ActionObjectId;
            rule.PermissionId = request.PermissionId;
            rule.IsAllowed = request.IsAllowed;
            rule.Priority = request.Priority;
            rule.IsInherited = request.IsInherited;
            rule.IsInheritable = request.IsInheritable;
            rule.IsActive = request.IsActive;
            rule.UpdatedAt = DateTime.UtcNow;

            await _ruleRepository.UpdateAsync(rule);
            return MapToDto(rule);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _ruleRepository.DeleteAsync(id);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _ruleRepository.ExistsAsync(id);
        }

        public async Task<bool> EvaluatePermissionAsync(Guid ruleId, Guid userId)
        {
            var rule = await _ruleRepository.GetByIdAsync(ruleId);
            if (rule == null) return false;

            // Build expression from rule tree and evaluate in DB
            var ruleExpr = rule.ToExpression();
            var param = ruleExpr.Parameters[0];
            var idCheck = Expression.Equal(
                Expression.Property(param, nameof(User.Id)),
                Expression.Constant(userId));
            var combined = Expression.AndAlso(idCheck, ruleExpr.Body);
            var predicate = Expression.Lambda<Func<User, bool>>(combined, param);

            var matches = await _userRepository.AnyMatchAsync(predicate);
            return matches && rule.IsAllowed;
        }

        private DynamicPermissionRuleDto MapToDto(DynamicPermissionRule rule)
        {
            var dto = new DynamicPermissionRuleDto
            {
                Id = rule.Id,
                Name = rule.Name,
                Description = rule.Description,
                Field = rule.Field,
                Operator = rule.Operator,
                Value = rule.Value,
                IsDynamicValue = rule.IsDynamicValue,
                RuleType = rule.RuleType,
                ParentRuleId = rule.ParentRuleId,
                UserGroupId = rule.UserGroupId,
                ActionObjectId = rule.ActionObjectId,
                PermissionId = rule.PermissionId,
                IsAllowed = rule.IsAllowed,
                Priority = rule.Priority,
                IsInherited = rule.IsInherited,
                IsInheritable = rule.IsInheritable,
                IsActive = rule.IsActive,
                CreatedAt = rule.CreatedAt,
                UpdatedAt = rule.UpdatedAt,
                CreatedById = rule.CreatedById,
                OrganizationId = rule.OrganizationId,
                UserGroupName = rule.UserGroup?.Name,
                ParentRuleName = rule.ParentRule?.Name,
                ActionObjectName = rule.ActionObject?.Name,
                PermissionName = rule.Permission?.Name
            };

            if (rule.ChildRules != null && rule.ChildRules.Any())
            {
                dto.ChildRules = rule.ChildRules.Select(MapToDto).ToList();
            }

            return dto;
        }
    }
}