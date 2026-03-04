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

        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
