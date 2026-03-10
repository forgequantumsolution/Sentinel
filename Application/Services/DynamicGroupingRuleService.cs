using Microsoft.EntityFrameworkCore;
using Core.Entities;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class DynamicGroupingRuleService : IDynamicGroupingRuleService
    {
        private readonly IDynamicGroupingRuleRepository _ruleRepository;
        private readonly IUserRepository _userRepository;

        public DynamicGroupingRuleService(
            IDynamicGroupingRuleRepository ruleRepository,
            IUserRepository userRepository)
        {
            _ruleRepository = ruleRepository;
            _userRepository = userRepository;
        }

        public async Task<DynamicGroupingRuleDto?> GetByIdAsync(Guid id)
        {
            var rule = await _ruleRepository.GetByIdAsync(id);
            if (rule == null) return null;

            return MapToDto(rule);
        }

        public async Task<List<DynamicGroupingRuleDto>> GetAllAsync()
        {
            var rules = await _ruleRepository.GetAllAsync();
            return rules.Select(MapToDto).ToList();
        }

        public async Task<List<DynamicGroupingRuleDto>> GetByUserGroupIdAsync(Guid userGroupId)
        {
            var rules = await _ruleRepository.GetByUserGroupIdAsync(userGroupId);
            return rules.Select(MapToDto).ToList();
        }

        public async Task<DynamicGroupingRuleDto> CreateAsync(CreateDynamicGroupingRuleRequest request)
        {
            var rule = new DynamicGroupingRule
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
                AutoAssign = request.AutoAssign,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            // Handle child rules if provided
            if (request.ChildRules != null && request.ChildRules.Any())
            {
                var childRules = new List<DynamicGroupingRule>();
                foreach (var childRequest in request.ChildRules)
                {
                    var childRule = new DynamicGroupingRule
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
                        AutoAssign = childRequest.AutoAssign,
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

        public async Task<DynamicGroupingRuleDto> UpdateAsync(Guid id, UpdateDynamicGroupingRuleRequest request)
        {
            var rule = await _ruleRepository.GetByIdAsync(id);
            if (rule == null)
                throw new KeyNotFoundException($"Dynamic grouping rule with ID {id} not found");

            rule.Name = request.Name;
            rule.Description = request.Description;
            rule.Field = request.Field;
            rule.Operator = request.Operator;
            rule.Value = request.Value;
            rule.IsDynamicValue = request.IsDynamicValue;
            rule.RuleType = request.RuleType;
            rule.ParentRuleId = request.ParentRuleId;
            rule.UserGroupId = request.UserGroupId;
            rule.AutoAssign = request.AutoAssign;
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

        public async Task<bool> UserMatchesRuleAsync(Guid ruleId, Guid userId)
        {
            var rule = await _ruleRepository.GetByIdAsync(ruleId);
            var user = await _userRepository.GetByIdAsync(userId);

            if (rule == null || user == null)
                return false;

            return rule.UserMatchesRule(user);
        }

        private DynamicGroupingRuleDto MapToDto(DynamicGroupingRule rule)
        {
            var dto = new DynamicGroupingRuleDto
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
                AutoAssign = rule.AutoAssign,
                IsActive = rule.IsActive,
                CreatedAt = rule.CreatedAt,
                UpdatedAt = rule.UpdatedAt,
                CreatedById = rule.CreatedById,
                OrganizationId = rule.OrganizationId,
                UserGroupName = rule.UserGroup?.Name,
                ParentRuleName = rule.ParentRule?.Name
            };

            if (rule.ChildRules != null && rule.ChildRules.Any())
            {
                dto.ChildRules = rule.ChildRules.Select(MapToDto).ToList();
            }

            return dto;
        }
    }
}