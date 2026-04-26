using Application.Common.Pagination;
using Core.Entities;
using Application.DTOs;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class DynamicGroupObjectPermissionService : IDynamicGroupObjectPermissionService
    {
        private readonly IDynamicGroupObjectPermissionRepository _ruleRepository;

        public DynamicGroupObjectPermissionService(IDynamicGroupObjectPermissionRepository ruleRepository)
        {
            _ruleRepository = ruleRepository;
        }

        public async Task<DynamicGroupObjectPermissionDto?> GetByIdAsync(Guid id)
        {
            var rule = await _ruleRepository.GetByIdAsync(id);
            if (rule == null) return null;
            return MapToDto(rule);
        }

        public async Task<PagedResult<DynamicGroupObjectPermissionDto>> GetAllAsync(PageRequest pageRequest)
        {
            return MapPaged(await _ruleRepository.GetAllAsync(pageRequest));
        }

        public async Task<PagedResult<DynamicGroupObjectPermissionDto>> GetByUserGroupIdAsync(Guid userGroupId, PageRequest pageRequest)
        {
            return MapPaged(await _ruleRepository.GetByUserGroupIdAsync(userGroupId, pageRequest));
        }

        public async Task<PagedResult<DynamicGroupObjectPermissionDto>> GetByActionObjectIdAsync(Guid actionObjectId, PageRequest pageRequest)
        {
            return MapPaged(await _ruleRepository.GetByActionObjectIdAsync(actionObjectId, pageRequest));
        }

        public async Task<PagedResult<DynamicGroupObjectPermissionDto>> GetByPermissionIdAsync(Guid permissionId, PageRequest pageRequest)
        {
            return MapPaged(await _ruleRepository.GetByPermissionIdAsync(permissionId, pageRequest));
        }

        public async Task<PagedResult<DynamicGroupObjectPermissionDto>> GetByActionObjectAndPermissionAsync(Guid actionObjectId, Guid permissionId, PageRequest pageRequest)
        {
            return MapPaged(await _ruleRepository.GetByActionObjectAndPermissionAsync(actionObjectId, permissionId, pageRequest));
        }

        private PagedResult<DynamicGroupObjectPermissionDto> MapPaged(PagedResult<DynamicGroupObjectPermission> paged)
        {
            return new PagedResult<DynamicGroupObjectPermissionDto>
            {
                Items = paged.Items.Select(MapToDto).ToList(),
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize
            };
        }

        public async Task<DynamicGroupObjectPermissionDto> CreateAsync(CreateDynamicGroupObjectPermissionRequest request)
        {
            var rule = new DynamicGroupObjectPermission
            {
                Name = request.Name,
                Description = request.Description,
                UserGroupId = request.UserGroupId,
                IsAllowed = request.IsAllowed,
                Priority = request.Priority,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            if (request.ActionObjectPermissionSets != null && request.ActionObjectPermissionSets.Any())
            {
                rule.ActionObjectPermissionSets = request.ActionObjectPermissionSets
                    .Select(s => new ActionObjectPermissionSet
                    {
                        DynamicGroupObjectPermissionId = rule.Id,
                        ActionObjectId = s.ActionObjectId,
                        Permissions = s.PermissionIds.Select(pid => new ActionObjectPermissionSetItem
                        {
                            PermissionId = pid
                        }).ToList()
                    }).ToList();
            }

            await _ruleRepository.AddAsync(rule);
            return MapToDto(rule);
        }

        public async Task<DynamicGroupObjectPermissionDto> UpdateAsync(Guid id, UpdateDynamicGroupObjectPermissionRequest request)
        {
            var rule = await _ruleRepository.GetByIdAsync(id);
            if (rule == null)
                throw new KeyNotFoundException($"Dynamic permission rule with ID {id} not found");

            rule.Name = request.Name;
            rule.Description = request.Description;
            rule.UserGroupId = request.UserGroupId;
            rule.IsAllowed = request.IsAllowed;
            rule.Priority = request.Priority;
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

        private DynamicGroupObjectPermissionDto MapToDto(DynamicGroupObjectPermission rule)
        {
            var dto = new DynamicGroupObjectPermissionDto
            {
                Id = rule.Id,
                Name = rule.Name,
                Description = rule.Description,
                UserGroupId = rule.UserGroupId,
                UserGroupName = rule.UserGroup?.Name,
                IsAllowed = rule.IsAllowed,
                Priority = rule.Priority,
                IsActive = rule.IsActive,
                CreatedAt = rule.CreatedAt,
                UpdatedAt = rule.UpdatedAt,
                CreatedById = rule.CreatedById,
                OrganizationId = rule.OrganizationId
            };

            if (rule.ActionObjectPermissionSets != null && rule.ActionObjectPermissionSets.Any())
            {
                dto.ActionObjectPermissionSets = rule.ActionObjectPermissionSets
                    .Select(s => new ActionObjectPermissionSetDto
                    {
                        Id = s.Id,
                        ActionObjectId = s.ActionObjectId,
                        ActionObjectName = s.ActionObject?.Name,
                        Permissions = s.Permissions?.Select(p => new ActionObjectPermissionSetItemDto
                        {
                            Id = p.Id,
                            PermissionId = p.PermissionId,
                            PermissionName = p.Permission?.Name
                        }).ToList() ?? []
                    }).ToList();
            }

            return dto;
        }
    }
}
