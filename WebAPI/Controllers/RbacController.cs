using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Common.Pagination;
using Application.DTOs;
using Application.Interfaces.Services;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RbacController : ControllerBase
    {
        private readonly IRbacService _rbacService;
        private readonly IUserService _userService;

        public RbacController(IRbacService rbacService, IUserService userService)
        {
            _rbacService = rbacService;
            _userService = userService;
        }

        /// <summary>
        /// Get all action object permissions assigned to a specific user
        /// </summary>
        [HttpGet("user/{userId}/permissions")]
        public async Task<IActionResult> GetUserPermissions(Guid userId, [FromQuery] PageRequest pageRequest)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var pagedAssignments = await _rbacService.GetUserAssignmentsAsync(userId, pageRequest);

            var result = new UserPermissionsDto
            {
                UserId = user.Id,
                UserName = user.Name,
                Email = user.Email,
                Permissions = pagedAssignments.Items
                    .Where(a => a.ActionObject != null && a.Permission != null)
                    .Select(a => new ActionObjectPermissionDto
                    {
                        ActionObjectId = a.ActionObject!.Id,
                        ActionObject = new ActionObjectDto
                        {
                            Id = a.ActionObject.Id,
                            Name = a.ActionObject.Name,
                            Code = a.ActionObject.Code,
                            Description = a.ActionObject.Description,
                            ObjectType = a.ActionObject.ObjectType.ToString(),
                            Route = a.ActionObject.Route,
                            Icon = a.ActionObject.Icon,
                            SortOrder = a.ActionObject.SortOrder,
                            ParentObjectId = a.ActionObject.ParentObjectId,
                            IsActive = a.ActionObject.IsActive,
                            CreatedAt = a.ActionObject.CreatedAt
                        },
                        PermissionId = a.Permission!.Id,
                        Permission = new AppPermissionDto
                        {
                            Id = a.Permission.Id,
                            Name = a.Permission.Name,
                            Code = a.Permission.Code,
                            Description = a.Permission.Description,
                            IsActive = a.Permission.IsActive
                        }
                    })
                    .ToList()
            };

            return Ok(result);
        }

        /// <summary>
        /// Get all action objects available in the system
        /// </summary>
        [HttpGet("action-objects")]
        public async Task<IActionResult> GetActionObjects([FromQuery] Guid? parentId, [FromQuery] PageRequest pageRequest)
        {
            var result = await _rbacService.GetActionObjectsAsync(parentId, pageRequest);

            return Ok(new PagedResult<ActionObjectDto>
            {
                Items = result.Items.Select(MapActionObject),
                TotalCount = result.TotalCount,
                Page = result.Page,
                PageSize = result.PageSize
            });
        }

        private static ActionObjectDto MapActionObject(Core.Entities.ActionObject a)
        {
            return new ActionObjectDto
            {
                Id = a.Id,
                Name = a.Name,
                Code = a.Code,
                Description = a.Description,
                ObjectType = a.ObjectType.ToString(),
                Route = a.Route,
                Icon = a.Icon,
                SortOrder = a.SortOrder,
                ParentObjectId = a.ParentObjectId,
                IsActive = a.IsActive,
                CreatedAt = a.CreatedAt,
                ChildObjects = a.ChildObjects?.Where(c => c.IsActive && !c.IsDeleted).Select(MapActionObject).ToList(),
                HasChildren = a.HasChildren
            };
        }

        /// <summary>
        /// Get all action object permissions assigned to a specific group
        /// </summary>
        [HttpGet("group/{groupId}/permissions")]
        public async Task<IActionResult> GetGroupPermissions(Guid groupId, [FromQuery] Guid? parentObjectId, [FromQuery] PageRequest pageRequest)
        {
            var result = await _rbacService.GetGroupAssignmentsAsync(groupId, parentObjectId, pageRequest);
            return Ok(result);
        }

        /// <summary>
        /// Get all permissions available in the system
        /// </summary>
        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions([FromQuery] PageRequest pageRequest)
        {
            var result = await _rbacService.GetAllPermissionsAsync(pageRequest);
            return Ok(result);
        }

        /// <summary>
        /// Check if a user has a specific action object permission
        /// </summary>
        [HttpGet("user/{userId}/has-permission")]
        public async Task<IActionResult> UserHasPermission(Guid userId, [FromQuery] string actionObjectCode, [FromQuery] string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(actionObjectCode) || string.IsNullOrWhiteSpace(permissionCode))
                return BadRequest(new { message = "actionObjectCode and permissionCode are required" });

            var hasPermission = await _rbacService.UserHasPermissionAsync(userId, actionObjectCode, permissionCode);

            return Ok(new { userId, actionObjectCode, permissionCode, hasPermission });
        }

        /// <summary>
        /// Assign an action object permission to a user or organization
        /// </summary>
        [HttpPost("assign")]
        [Authorize(Roles = "admin,sys-admin")]
        public async Task<IActionResult> Assign([FromBody] AssignPermissionRequest request)
        {
            try
            {
                var assignment = await _rbacService.AssignAsync(
                    request.ActionObjectId,
                    request.PermissionId,
                    request.AssigneeType,
                    request.AssigneeId
                );

                return Ok(new
                {
                    message = "Permission assigned successfully",
                    assignment = new
                    {
                        assignment.Id,
                        assignment.ActionObjectId,
                        assignment.PermissionId,
                        assignment.AssigneeType,
                        assignment.AssigneeId,
                        assignment.IsActive
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Revoke an action object permission from a user or organization
        /// </summary>
        [HttpPost("revoke")]
        [Authorize(Roles = "admin,sys-admin")]
        public async Task<IActionResult> Revoke([FromBody] RevokePermissionRequest request)
        {
            try
            {
                await _rbacService.RevokeAsync(
                    request.ActionObjectId,
                    request.PermissionId,
                    request.AssigneeType,
                    request.AssigneeId
                );

                return Ok(new { message = "Permission revoked successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class AssignPermissionRequest
    {
        public Guid ActionObjectId { get; set; }
        public Guid PermissionId { get; set; }
        public Core.Enums.AssigneeType AssigneeType { get; set; }
        public Guid AssigneeId { get; set; }
    }

    public class RevokePermissionRequest
    {
        public Guid ActionObjectId { get; set; }
        public Guid PermissionId { get; set; }
        public Core.Enums.AssigneeType AssigneeType { get; set; }
        public Guid AssigneeId { get; set; }
    }
}
