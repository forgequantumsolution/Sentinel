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

                // Get user's action object permission assignments
                var pagedAssignments = await _rbacService.GetUserAssignmentsAsync(userId, new Application.Common.Pagination.PageRequest { Page = 1, PageSize = 100 });

                // Map to DTOs
                var permissions = pagedAssignments.Items
                    .Select(a => new ActionObjectPermissionDto
                    {
                        ActionObjectId = a.ActionObject.Id,
                        ActionObject = new ActionObjectDto
                        {
                            Id = a.ActionObject.Id,
                            Name = a.ActionObject.Name,
                            Code = a.ActionObject.Code,
                            Description = a.ActionObject.Description,
                            ObjectType = a.ActionObject.ObjectType.ToString(),
                            Route = a.ActionObject.Route,
                            ParentObjectId = a.ActionObject.ParentObjectId,
                            IsActive = a.ActionObject.IsActive,
                            CreatedAt = a.ActionObject.CreatedAt
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
                    Permissions = permissions
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
