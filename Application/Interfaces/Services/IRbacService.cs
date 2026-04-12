using Application.Common.Pagination;
using Core.Entities;
using Core.Enums;

namespace Application.Interfaces.Services
{
    public interface IRbacService
    {
        // ── ActionObject Queries ──
        Task<PagedResult<ActionObject>> GetActionObjectsAsync(Guid? parentId, PageRequest pageRequest);

        // ── AppPermission CRUD ──
        Task<AppPermission> CreatePermissionAsync(AppPermission permission);
        Task<AppPermission?> GetPermissionByIdAsync(Guid id);
        Task<PagedResult<AppPermission>> GetAllPermissionsAsync(PageRequest pageRequest);
        Task UpdatePermissionAsync(AppPermission permission);
        Task DeletePermissionAsync(Guid id);

        // ── Assignment ──
        Task<ActionObjectPermissionAssignment> AssignAsync(Guid actionObjectId, Guid permissionId, AssigneeType assigneeType, Guid assigneeId);
        Task RevokeAsync(Guid actionObjectId, Guid permissionId, AssigneeType assigneeType, Guid assigneeId);

        // ── Queries ──
        Task<bool> UserHasPermissionAsync(Guid userId, string actionObjectCode, string permissionCode);
        Task<bool> OrgHasPermissionAsync(Guid orgId, Guid actionObjectId, Guid permissionId);
        Task<PagedResult<ActionObjectPermissionAssignment>> GetUserAssignmentsAsync(Guid userId, PageRequest pageRequest);
        Task<PagedResult<ActionObjectPermissionAssignment>> GetOrgAssignmentsAsync(Guid orgId, PageRequest pageRequest);
    }
}
