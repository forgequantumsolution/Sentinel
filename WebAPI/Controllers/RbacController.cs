using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        /// Get all features and permissions assigned to a specific user
        /// </summary>
        [HttpGet("user/{userId}/features-permissions")]
        public async Task<IActionResult> GetUserFeaturesPermissions(Guid userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            // Get user's feature-permission assignments
            var assignments = await _rbacService.GetUserAssignmentsAsync(userId);

            // Map to DTOs
            var filteredAssignments = assignments
                .Where(a => a.IsActive && !a.IsDeleted)
                .Select(a => new FeaturePermissionDto
                {
                    FeatureId = a.Feature.Id,
                    Feature = new FeatureDto
                    {
                        Id = a.Feature.Id,
                        Name = a.Feature.Name,
                        Code = a.Feature.Code,
                        Description = a.Feature.Description,
                        ParentFeatureId = a.Feature.ParentFeatureId,
                        IsActive = a.Feature.IsActive
                    },
                    PermissionId = a.Permission.Id,
                    Permission = new AppPermissionDto
                    {
                        Id = a.Permission.Id,
                        Name = a.Permission.Name,
                        Code = a.Permission.Code,
                        Description = a.Permission.Description,
                        IsActive = a.Permission.IsActive
                    }
                })
                .ToList();

            var response = new UserFeaturesPermissionsDto
            {
                UserId = user.Id,
                UserName = user.Name,
                Email = user.Email,
                FeaturesPermissions = filteredAssignments
            };

            return Ok(response);
        }

        /// <summary>
        /// Get all features available in the system
        /// </summary>
        [HttpGet("features")]
        public async Task<IActionResult> GetAllFeatures()
        {
            var features = await _rbacService.GetAllFeaturesAsync();

            var dtos = features
                .Where(f => f.IsActive && !f.IsDeleted)
                .Select(f => new FeatureDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    Code = f.Code,
                    Description = f.Description,
                    ParentFeatureId = f.ParentFeatureId,
                    IsActive = f.IsActive
                });

            return Ok(dtos);
        }

        /// <summary>
        /// Get all permissions available in the system
        /// </summary>
        [HttpGet("permissions")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _rbacService.GetAllPermissionsAsync();

            var dtos = permissions
                .Where(p => p.IsActive && !p.IsDeleted)
                .Select(p => new AppPermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Description = p.Description,
                    IsActive = p.IsActive
                });

            return Ok(dtos);
        }

        /// <summary>
        /// Check if a user has a specific feature permission
        /// </summary>
        [HttpGet("user/{userId}/has-permission")]
        public async Task<IActionResult> UserHasPermission(Guid userId, [FromQuery] string featureCode, [FromQuery] string permissionCode)
        {
            if (string.IsNullOrWhiteSpace(featureCode) || string.IsNullOrWhiteSpace(permissionCode))
                return BadRequest(new { message = "featureCode and permissionCode are required" });

            var hasPermission = await _rbacService.UserHasPermissionAsync(userId, featureCode, permissionCode);

            return Ok(new { userId, featureCode, permissionCode, hasPermission });
        }

        /// <summary>
        /// Assign a feature-permission to a user
        /// </summary>
        [HttpPost("assign")]
        [Authorize(Roles = "admin,sys-admin")]
        public async Task<IActionResult> AssignPermissionToUser([FromBody] AssignPermissionRequest request)
        {
            try
            {
                var assignment = await _rbacService.AssignAsync(
                    request.FeatureId,
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
                        assignment.FeatureId,
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
        /// Revoke a feature-permission from a user
        /// </summary>
        [HttpPost("revoke")]
        [Authorize(Roles = "admin,sys-admin")]
        public async Task<IActionResult> RevokePermissionFromUser([FromBody] RevokePermissionRequest request)
        {
            try
            {
                await _rbacService.RevokeAsync(
                    request.FeatureId,
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

    // Request/Response models for RBAC operations
    public class AssignPermissionRequest
    {
        public Guid FeatureId { get; set; }
        public Guid PermissionId { get; set; }
        public Core.Enums.AssigneeType AssigneeType { get; set; }
        public Guid AssigneeId { get; set; }
    }

    public class RevokePermissionRequest
    {
        public Guid FeatureId { get; set; }
        public Guid PermissionId { get; set; }
        public Core.Enums.AssigneeType AssigneeType { get; set; }
        public Guid AssigneeId { get; set; }
    }
}
