using Microsoft.EntityFrameworkCore;
using Application.Common.Pagination;
using Application.DTOs;
using Application.Interfaces.Services;
using Core.Entities;
using Core.Enums;
using Infrastructure.Persistence;

namespace Infrastructure.Services
{
    public class RbacService : IRbacService
    {
        private readonly AppDbContext _context;

        public RbacService(AppDbContext context)
        {
            _context = context;
        }

        // ════════════════════════════════════════════
        //  ActionObject Queries
        // ════════════════════════════════════════════

        public async Task<PagedResult<ActionObject>> GetActionObjectsAsync(Guid? parentId, PageRequest pageRequest)
        {
            var query = _context.ActionObjects
                .Include(x => x.ChildObjects)
                .Include(x => x.Department)
                .Where(a => !a.IsDeleted && a.IsActive && a.ParentObjectId == parentId)
                .Where(a => a.ObjectType == ObjectType.Folder || a.ObjectType == ObjectType.Feature)
                .OrderBy(a => a.SortOrder);

            var totalCount = await query.CountAsync();
            var items = await query.Skip(pageRequest.Skip).Take(pageRequest.PageSize).ToListAsync();

            // Single query: find which of these items have active children
            var itemIds = items.Select(i => i.Id).ToList();
            var idsWithChildren = await _context.ActionObjects
                .Where(a => a.ParentObjectId != null && itemIds.Contains(a.ParentObjectId.Value) && a.IsActive && !a.IsDeleted)
                .Select(a => a.ParentObjectId!.Value)
                .Distinct()
                .ToListAsync();

            var hasChildrenSet = new HashSet<Guid>(idsWithChildren);
            foreach (var item in items)
            {
                item.HasChildren = hasChildrenSet.Contains(item.Id);
            }

            return new PagedResult<ActionObject>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }

        // ════════════════════════════════════════════
        //  AppPermission CRUD
        // ════════════════════════════════════════════

        public async Task<AppPermission> CreatePermissionAsync(AppPermission permission)
        {
            await _context.AppPermissions.AddAsync(permission);
            await _context.SaveChangesAsync();
            return permission;
        }

        public async Task<AppPermission?> GetPermissionByIdAsync(Guid id)
        {
            return await _context.AppPermissions
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
        }

        public async Task<PagedResult<AppPermission>> GetAllPermissionsAsync(PageRequest pageRequest)
        {
            var query = _context.AppPermissions
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.Name);

            var totalCount = await query.CountAsync();
            var items = await query.Skip(pageRequest.Skip).Take(pageRequest.PageSize).ToListAsync();

            return new PagedResult<AppPermission>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }

        public async Task UpdatePermissionAsync(AppPermission permission)
        {
            permission.UpdatedAt = DateTime.UtcNow;
            _context.AppPermissions.Update(permission);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePermissionAsync(Guid id)
        {
            var perm = await _context.AppPermissions.FindAsync(id);
            if (perm != null)
            {
                perm.IsDeleted = true;
                perm.IsActive = false;
                perm.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        // ════════════════════════════════════════════
        //  Assignment / Revocation
        // ════════════════════════════════════════════

        public async Task<ActionObjectPermissionAssignment> AssignAsync(
            Guid actionObjectId, Guid permissionId, AssigneeType assigneeType, Guid assigneeId)
        {
            // Direct user-level assignments are no longer supported.
            // Permissions flow: Organization (ceiling) → Group → Users (via dynamic group membership).
            if (assigneeType == AssigneeType.User)
                throw new InvalidOperationException(
                    "Direct user-level permission assignment is not supported. " +
                    "Assign permissions to a UserGroup instead.");

            // Business rule: group can only get what its org already has
            if (assigneeType == AssigneeType.Group)
            {
                var group = await _context.UserGroups
                    .IgnoreQueryFilters()
                    .Include(g => g.Role)
                    .FirstOrDefaultAsync(g => g.Id == assigneeId);

                if (group == null)
                    throw new InvalidOperationException("UserGroup not found.");

                if (!group.OrganizationId.HasValue)
                    throw new InvalidOperationException("UserGroup does not belong to any organization.");

                EnsureNotAdminRoleGroup(group);

                //await EnsureOrgHasPermissionAsync(actionObjectId, permissionId, group.OrganizationId.Value, "group");
            }

            // Check for existing assignment — include soft-deleted to reuse the row
            var existing = await _context.ActionObjectPermissionAssignments
                .FirstOrDefaultAsync(a =>
                    a.ActionObjectId == actionObjectId &&
                    a.PermissionId == permissionId &&
                    a.AssigneeType == assigneeType &&
                    a.AssigneeId == assigneeId);

            if (existing != null)
            {
                // Reactivate (handles both inactive and previously-revoked rows)
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return existing;
            }

            var assignment = new ActionObjectPermissionAssignment
            {
                ActionObjectId = actionObjectId,
                PermissionId = permissionId,
                AssigneeType = assigneeType,
                AssigneeId = assigneeId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.ActionObjectPermissionAssignments.AddAsync(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        private static readonly string[] AdminRoleNames = ["super-admin", "sys-admin", "admin"];

        private static void EnsureNotAdminRoleGroup(UserGroup group)
        {
            if (group.Role != null && AdminRoleNames.Contains(group.Role.Name))
                throw new InvalidOperationException(
                    $"Permissions for the '{group.Role.Name}' role group cannot be modified. " +
                    "Admin role groups are granted all permissions automatically.");
        }

        private async Task EnsureOrgHasPermissionAsync(Guid actionObjectId, Guid permissionId, Guid orgId, string assigneeLabel)
        {
            var orgHasAccess = await _context.ActionObjectPermissionAssignments
                .IgnoreQueryFilters()
                .AnyAsync(a =>
                    a.ActionObjectId == actionObjectId &&
                    a.PermissionId == permissionId &&
                    a.AssigneeType == AssigneeType.Organization &&
                    a.AssigneeId == orgId &&
                    a.IsActive && !a.IsDeleted);

            if (!orgHasAccess)
                throw new InvalidOperationException(
                    $"Cannot assign this permission to the {assigneeLabel} because " +
                    "their organization does not have access to it.");
        }

        public async Task RevokeAsync(
            Guid actionObjectId, Guid permissionId, AssigneeType assigneeType, Guid assigneeId)
        {
            // Block revoking from admin role groups (their permissions are managed by the system).
            if (assigneeType == AssigneeType.Group)
            {
                var group = await _context.UserGroups
                    .IgnoreQueryFilters()
                    .Include(g => g.Role)
                    .FirstOrDefaultAsync(g => g.Id == assigneeId);

                if (group != null) EnsureNotAdminRoleGroup(group);
            }

            var assignment = await _context.ActionObjectPermissionAssignments
                .FirstOrDefaultAsync(a =>
                    a.ActionObjectId == actionObjectId &&
                    a.PermissionId == permissionId &&
                    a.AssigneeType == assigneeType &&
                    a.AssigneeId == assigneeId && !a.IsDeleted);

            if (assignment != null)
            {
                assignment.IsActive = false;
                assignment.UpdatedAt = DateTime.UtcNow;
            }

            // Cascade: if revoking from an Org, also revoke from all groups in that Org.
            // Users get permissions only via groups, so revoking groups removes user access automatically.
            if (assigneeType == AssigneeType.Organization)
            {
                var groupAssignments = await _context.ActionObjectPermissionAssignments
                    .Where(a =>
                        a.ActionObjectId == actionObjectId &&
                        a.PermissionId == permissionId &&
                        a.AssigneeType == AssigneeType.Group &&
                        a.OrganizationId == assigneeId &&
                        !a.IsDeleted)
                    .ToListAsync();

                foreach (var d in groupAssignments)
                {
                    d.IsActive = false;
                    d.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }

        // ════════════════════════════════════════════
        //  Queries
        // ════════════════════════════════════════════

        public async Task<bool> UserHasPermissionAsync(Guid userId, string actionObjectCode, string permissionCode)
        {
            // User has a permission if any of their groups (via SQL VIEW) was granted it.
            var userGroupIds = _context.UserGroupMemberships
                .Where(m => m.UserId == userId)
                .Select(m => m.UserGroupId);

            return await _context.UserGroupMemberships
                .Include(a => a.ActionObject)
                .Include(a => a.Permission)
                .AnyAsync(a =>
                    a.ActionObject.Code == actionObjectCode &&
                    a.Permission.Code == permissionCode &&
                    userGroupIds.Contains(a.UserGroupId));
        }

        public async Task<bool> OrgHasPermissionAsync(Guid orgId, Guid actionObjectId, Guid permissionId)
        {
            return await _context.ActionObjectPermissionAssignments
                .IgnoreQueryFilters()
                .AnyAsync(a =>
                    a.ActionObjectId == actionObjectId &&
                    a.PermissionId == permissionId &&
                    a.AssigneeType == AssigneeType.Organization &&
                    a.AssigneeId == orgId &&
                    a.IsActive && !a.IsDeleted);
        }

        public async Task<PagedResult<ActionObjectWithPermissionsDto>> GetUserAssignmentsAsync(Guid userId, Guid? parentObjectId, PageRequest pageRequest)
        {
            // Effective permissions for the user from the SQL VIEW (vw_UserGroupMemberships)
            // — full User → Group → ActionObject + Permission chain, including admin overrides.
            // Filter to ActionObjects under the requested parent (or root if null).
            // Same shape as GetGroupAssignmentsAsync.
            var baseQuery = _context.UserGroupMemberships
                .Where(m => m.UserId == userId
                         && m.ActionObjectId != null
                         && m.PermissionId != null);

            // Step 1: page distinct ActionObjectIds — restricted to the requested parent level.
            // Exclude Url-type ActionObjects (those are API endpoints, not UI features).
            var actionObjectsAtLevel = _context.ActionObjects
                .Where(a => a.ParentObjectId == parentObjectId
                         && a.IsActive && !a.IsDeleted
                         && (a.ObjectType != ObjectType.Url))
                .Select(a => a.Id);

            var distinctActionObjects = baseQuery
                .Select(m => m.ActionObjectId!.Value)
                .Where(id => actionObjectsAtLevel.Contains(id))
                .Distinct();

            var totalCount = await distinctActionObjects.CountAsync();

            var pageActionObjectIds = await distinctActionObjects
                .OrderBy(id => id)
                .Skip(pageRequest.Skip)
                .Take(pageRequest.PageSize)
                .ToListAsync();

            // Step 2: get all (ActionObjectId, PermissionId) pairs for these ActionObjects.
            var pairs = await baseQuery
                //.Where(m => pageActionObjectIds.Contains(m.ActionObjectId!.Value))
                .Select(m => new { ActionObjectId = m.ActionObjectId!.Value, PermissionId = m.PermissionId!.Value })
                .Distinct()
                .ToListAsync();

            // Step 3: hydrate ActionObject + Permission entities.
            var permissionIds = pairs.Select(p => p.PermissionId).Distinct().ToList();

            var actionObjects = await _context.ActionObjects
                .Include(a => a.Department)
                .Where(a => pageActionObjectIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id);

            var permissions = await _context.AppPermissions
                .Where(p => permissionIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            // Step 4: load first-level children (assigned to this user) and their permissions.
            var childrenByParent = new Dictionary<Guid, List<ActionObject>>();
            var childPairs = new List<(Guid ActionObjectId, Guid PermissionId)>();
            if (pageActionObjectIds.Count > 0)
            {
                var children = await _context.ActionObjects
                    .Include(c => c.Department)
                    .Where(c => c.ParentObjectId != null
                             && baseQuery.Select(x => x.ActionObjectId).Distinct().Contains(c.Id)
                             && pageActionObjectIds.Contains(c.ParentObjectId.Value)
                             && c.IsActive && !c.IsDeleted
                             && c.ObjectType != ObjectType.Url)
                    .ToListAsync();

                childrenByParent = children
                    .GroupBy(c => c.ParentObjectId!.Value)
                    .ToDictionary(g => g.Key, g => g.ToList());

                var childIds = children.Select(c => c.Id).ToList();
                if (childIds.Count > 0)
                {
                    childPairs = (await baseQuery
                        .Where(m => childIds.Contains(m.ActionObjectId!.Value))
                        .Select(m => new { ActionObjectId = m.ActionObjectId!.Value, PermissionId = m.PermissionId!.Value })
                        .Distinct()
                        .ToListAsync())
                        .Select(p => (p.ActionObjectId, p.PermissionId))
                        .ToList();

                    // Hydrate any permissions referenced only by children (not in parent set).
                    var missingPermIds = childPairs.Select(p => p.PermissionId)
                        .Distinct()
                        .Where(id => !permissions.ContainsKey(id))
                        .ToList();
                    if (missingPermIds.Count > 0)
                    {
                        var morePerms = await _context.AppPermissions
                            .Where(p => missingPermIds.Contains(p.Id))
                            .ToListAsync();
                        foreach (var p in morePerms)
                            permissions[p.Id] = p;
                    }
                }
            }

            // Step 5: assemble DTOs preserving page order.
            var items = pageActionObjectIds.Select(aoId =>
            {
                var ao = actionObjects.GetValueOrDefault(aoId);
                var aoPermissionIds = pairs
                    .Where(p => p.ActionObjectId == aoId)
                    .Select(p => p.PermissionId);

                return new ActionObjectWithPermissionsDto
                {
                    ActionObjectId = aoId,
                    ActionObject = ao == null ? null : new ActionObjectDto
                    {
                        Id = ao.Id,
                        Name = ao.Name,
                        Code = ao.Code,
                        Description = ao.Description,
                        ObjectType = ao.ObjectType.ToString(),
                        Route = ao.Route,
                        Icon = ao.Icon,
                        SortOrder = ao.SortOrder,
                        ParentObjectId = ao.ParentObjectId,
                        DepartmentId = ao.DepartmentId,
                        DepartmentName = ao.Department?.Name,
                        IsActive = ao.IsActive,
                        CreatedAt = ao.CreatedAt,
                        ChildObjects = childrenByParent.TryGetValue(aoId, out var kids)
                            ? kids.Select(c => new ActionObjectWithPermissionsDto
                            {
                                ActionObjectId = c.Id,
                                ActionObject = new ActionObjectDto
                                {
                                    Id = c.Id,
                                    Name = c.Name,
                                    Code = c.Code,
                                    Description = c.Description,
                                    ObjectType = c.ObjectType.ToString(),
                                    Route = c.Route,
                                    Icon = c.Icon,
                                    SortOrder = c.SortOrder,
                                    ParentObjectId = c.ParentObjectId,
                                    DepartmentId = c.DepartmentId,
                                    DepartmentName = c.Department?.Name,
                                    IsActive = c.IsActive,
                                    CreatedAt = c.CreatedAt
                                },
                                Permissions = childPairs
                                    .Where(p => p.ActionObjectId == c.Id && permissions.ContainsKey(p.PermissionId))
                                    .Select(p =>
                                    {
                                        var perm = permissions[p.PermissionId];
                                        return new AppPermissionDto
                                        {
                                            Id = perm.Id,
                                            Name = perm.Name,
                                            Code = perm.Code,
                                            Description = perm.Description,
                                            IsActive = perm.IsActive
                                        };
                                    })
                                    .ToList()
                            }).ToList()
                            : null
                    },
                    Permissions = aoPermissionIds
                        .Where(pid => permissions.ContainsKey(pid))
                        .Select(pid =>
                        {
                            var perm = permissions[pid];
                            return new AppPermissionDto
                            {
                                Id = perm.Id,
                                Name = perm.Name,
                                Code = perm.Code,
                                Description = perm.Description,
                                IsActive = perm.IsActive
                            };
                        })
                        .ToList()
                };
            }).ToList();

            return new PagedResult<ActionObjectWithPermissionsDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }

        public async Task<PagedResult<ActionObjectWithPermissionsDto>> GetGroupAssignmentsAsync(Guid groupId, Guid? parentObjectId, PageRequest pageRequest)
        {
            // Effective permissions for the group from the SQL VIEW.
            // Filter to ActionObjects under the requested parent (or root if null).
            var baseQuery = _context.UserGroupMemberships
                .Where(m => m.UserGroupId == groupId
                         && m.ActionObjectId != null
                         && m.PermissionId != null);

            // Step 1: page distinct ActionObjectIds — restricted to the requested parent level
            // Exclude Url-type ActionObjects (those are API endpoints, not UI features).
            var actionObjectsAtLevel = _context.ActionObjects
                .Where(a => a.ParentObjectId == parentObjectId
                         && a.IsActive && !a.IsDeleted
                         && (a.ObjectType != ObjectType.Url))
                .Select(a => a.Id);

            var distinctActionObjects = baseQuery
                .Select(m => m.ActionObjectId!.Value)
                .Where(id => actionObjectsAtLevel.Contains(id))
                .Distinct();

            var totalCount = await distinctActionObjects.CountAsync();

            var pageActionObjectIds = await distinctActionObjects
                .OrderBy(id => id)
                .Skip(pageRequest.Skip)
                .Take(pageRequest.PageSize)
                .ToListAsync();

            // Step 2: get all (ActionObjectId, PermissionId) pairs for these ActionObjects
            var pairs = await baseQuery
                .Where(m => pageActionObjectIds.Contains(m.ActionObjectId!.Value))
                .Select(m => new { ActionObjectId = m.ActionObjectId!.Value, PermissionId = m.PermissionId!.Value })
                .Distinct()
                .ToListAsync();

            // Step 3: hydrate ActionObject + Permission entities
            var permissionIds = pairs.Select(p => p.PermissionId).Distinct().ToList();

            var actionObjects = await _context.ActionObjects
                .Include(a => a.Department)
                .Where(a => pageActionObjectIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id);

            var permissions = await _context.AppPermissions
                .Where(p => permissionIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            // Step 4: assemble DTOs preserving page order
            var items = pageActionObjectIds.Select(aoId =>
            {
                var ao = actionObjects.GetValueOrDefault(aoId);
                var aoPermissionIds = pairs
                    .Where(p => p.ActionObjectId == aoId)
                    .Select(p => p.PermissionId);

                return new ActionObjectWithPermissionsDto
                {
                    ActionObjectId = aoId,
                    ActionObject = ao == null ? null : new ActionObjectDto
                    {
                        Id = ao.Id,
                        Name = ao.Name,
                        Code = ao.Code,
                        Description = ao.Description,
                        ObjectType = ao.ObjectType.ToString(),
                        Route = ao.Route,
                        Icon = ao.Icon,
                        SortOrder = ao.SortOrder,
                        ParentObjectId = ao.ParentObjectId,
                        DepartmentId = ao.DepartmentId,
                        DepartmentName = ao.Department?.Name,
                        IsActive = ao.IsActive,
                        CreatedAt = ao.CreatedAt
                    },
                    Permissions = aoPermissionIds
                        .Where(pid => permissions.ContainsKey(pid))
                        .Select(pid =>
                        {
                            var perm = permissions[pid];
                            return new AppPermissionDto
                            {
                                Id = perm.Id,
                                Name = perm.Name,
                                Code = perm.Code,
                                Description = perm.Description,
                                IsActive = perm.IsActive
                            };
                        })
                        .ToList()
                };
            }).ToList();

            return new PagedResult<ActionObjectWithPermissionsDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }

        public async Task<PagedResult<ActionObjectPermissionAssignment>> GetOrgAssignmentsAsync(Guid orgId, PageRequest pageRequest)
        {
            var query = _context.ActionObjectPermissionAssignments
                .IgnoreQueryFilters()
                .Include(a => a.ActionObject)
                .Include(a => a.Permission)
                .Where(a =>
                    a.AssigneeType == AssigneeType.Organization &&
                    a.AssigneeId == orgId &&
                    a.IsActive && !a.IsDeleted);

            var totalCount = await query.CountAsync();
            var items = await query.Skip(pageRequest.Skip).Take(pageRequest.PageSize).ToListAsync();

            return new PagedResult<ActionObjectPermissionAssignment>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }
    }
}
