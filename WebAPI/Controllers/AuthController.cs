using Microsoft.AspNetCore.Mvc;
using Analytics_BE.Application.Interfaces;
using Analytics_BE.Application.DTOs;

namespace Analytics_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;

        public AuthController(IAuthService authService, IUserService userService)
        {
            _authService = authService;
            _userService = userService;
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
                    user.Status
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
