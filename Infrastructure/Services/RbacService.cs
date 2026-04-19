using Microsoft.EntityFrameworkCore;
using Application.Common.Pagination;
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
                    .FirstOrDefaultAsync(g => g.Id == assigneeId);

                if (group == null)
                    throw new InvalidOperationException("UserGroup not found.");

                if (!group.OrganizationId.HasValue)
                    throw new InvalidOperationException("UserGroup does not belong to any organization.");

                await EnsureOrgHasPermissionAsync(actionObjectId, permissionId, group.OrganizationId.Value, "group");
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

        public async Task<PagedResult<ActionObjectPermissionAssignment>> GetUserAssignmentsAsync(Guid userId, PageRequest pageRequest)
        {
            // User's effective permissions are the union of permissions assigned to their groups (via SQL VIEW).
            var userGroupIds = _context.UserGroupMemberships
                .Where(m => m.UserId == userId)
                .Select(m => m.UserGroupId);

            var query = _context.ActionObjectPermissionAssignments
                .Include(a => a.ActionObject)
                .Include(a => a.Permission)
                .Where(a =>
                    a.AssigneeType == AssigneeType.Group &&
                    userGroupIds.Contains(a.AssigneeId) &&
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

        public async Task<PagedResult<ActionObjectPermissionAssignment>> GetGroupAssignmentsAsync(Guid groupId, PageRequest pageRequest)
        {
            var query = _context.ActionObjectPermissionAssignments
                .Include(a => a.ActionObject)
                .Include(a => a.Permission)
                .Where(a =>
                    a.AssigneeType == AssigneeType.Group &&
                    a.AssigneeId == groupId &&
                    a.IsActive && !a.IsDeleted)
                .OrderBy(a => a.ActionObject.Name);

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
