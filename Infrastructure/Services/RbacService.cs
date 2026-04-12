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
            // Business rule: user can only get what their org already has
            if (assigneeType == AssigneeType.User)
            {
                var user = await _context.Users
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.Id == assigneeId);

                if (user == null)
                    throw new InvalidOperationException("User not found.");

                if (!user.OrganizationId.HasValue)
                    throw new InvalidOperationException("User does not belong to any organization.");

                var orgHasAccess = await _context.ActionObjectPermissionAssignments
                    .IgnoreQueryFilters()
                    .AnyAsync(a =>
                        a.ActionObjectId == actionObjectId &&
                        a.PermissionId == permissionId &&
                        a.AssigneeType == AssigneeType.Organization &&
                        a.AssigneeId == user.OrganizationId.Value &&
                        a.IsActive && !a.IsDeleted);

                if (!orgHasAccess)
                    throw new InvalidOperationException(
                        "Cannot assign this permission to the user because " +
                        "their organization does not have access to it.");
            }

            // Check for existing assignment
            var existing = await _context.ActionObjectPermissionAssignments
                .FirstOrDefaultAsync(a =>
                    a.ActionObjectId == actionObjectId &&
                    a.PermissionId == permissionId &&
                    a.AssigneeType == assigneeType &&
                    a.AssigneeId == assigneeId && !a.IsDeleted);

            if (existing != null)
            {
                if (!existing.IsActive)
                {
                    existing.IsActive = true;
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }
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
                assignment.IsDeleted = true;
                assignment.DeletedAt = DateTime.UtcNow;
                assignment.UpdatedAt = DateTime.UtcNow;
            }

            // Cascade: if revoking from an Org, also revoke from all users in that Org
            if (assigneeType == AssigneeType.Organization)
            {
                var userAssignments = await _context.ActionObjectPermissionAssignments
                    .Where(a =>
                        a.ActionObjectId == actionObjectId &&
                        a.PermissionId == permissionId &&
                        a.AssigneeType == AssigneeType.User &&
                        a.OrganizationId == assigneeId &&
                        !a.IsDeleted)
                    .ToListAsync();

                foreach (var ua in userAssignments)
                {
                    ua.IsActive = false;
                    ua.IsDeleted = true;
                    ua.DeletedAt = DateTime.UtcNow;
                    ua.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }

        // ════════════════════════════════════════════
        //  Queries
        // ════════════════════════════════════════════

        public async Task<bool> UserHasPermissionAsync(Guid userId, string actionObjectCode, string permissionCode)
        {
            return await _context.ActionObjectPermissionAssignments
                .Include(a => a.ActionObject)
                .Include(a => a.Permission)
                .AnyAsync(a =>
                    a.ActionObject.Code == actionObjectCode &&
                    a.Permission.Code == permissionCode &&
                    a.AssigneeType == AssigneeType.User &&
                    a.AssigneeId == userId &&
                    a.IsActive && !a.IsDeleted);
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
            var query = _context.ActionObjectPermissionAssignments
                .Include(a => a.ActionObject)
                .Include(a => a.Permission)
                .Where(a =>
                    a.AssigneeType == AssigneeType.User &&
                    a.AssigneeId == userId &&
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
