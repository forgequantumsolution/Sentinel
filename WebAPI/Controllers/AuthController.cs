using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Services;
using Application.DTOs;
using Application.Interfaces.Services;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IRbacService _rbacService;

        public AuthController(IAuthService authService, IUserService userService, IRbacService rbacService)
        {
            _authService = authService;
            _userService = userService;
            _rbacService = rbacService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _authService.RegisterAsync(request);
                return Ok(new
                {
                    User = new
                    {
                        user.Id,
                        user.Name,
                        user.Email,
                        Role = user.Role?.Name,
                        Department = user.Department?.Name
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var (user, token) = await _authService.LoginAsync(request);
                return Ok(new
                {
                    User = new
                    {
                        user.Id,
                        user.Name,
                        user.Email,
                        Role = user.Role?.Name,
                        Department = user.Department?.Name
                    },
                    token = token
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized("User ID claim not found");
                }

                if (!Guid.TryParse(userIdClaim, out var userId))
                {
                    return BadRequest("Invalid User ID format");
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                // Get user's feature-permission assignments
                var assignments = await _rbacService.GetUserAssignmentsAsync(userId);

                // Map to DTOs
                var featuresPermissions = assignments
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

                return Ok(new
                {
                    user.Id,
                    user.Name,
                    user.Email,
                    Role = user.Role?.Name,
                    Department = user.Department?.Name,
                    user.JobTitleId,
                    JobTitle = user.JobTitle?.Title,
                    user.Location,
                    user.EmployeeId,
                    user.Status,
                    FeaturesPermissions = featuresPermissions
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
