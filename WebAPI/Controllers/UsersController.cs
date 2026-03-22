using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Core.Entities;
using Application.Interfaces.Services;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            var dtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                RoleId = u.RoleId,
                RoleName = u.Role?.Name,
                DepartmentId = u.DepartmentId,
                DepartmentName = u.Department?.Name,
                JobTitleId = u.JobTitleId,
                JobTitle = u.JobTitle?.Title,
                Location = u.Location,
                EmployeeId = u.EmployeeId,
                Status = (int)u.Status,
                IsActive = u.IsActive
            });
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            var dto = new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.Name,
                JobTitleId = user.JobTitleId,
                JobTitle = user.JobTitle?.Title,
                Location = user.Location,
                EmployeeId = user.EmployeeId,
                Status = (int)user.Status,
                IsActive = user.IsActive
            };
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
        {
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                RoleId = request.RoleId,
                DepartmentId = request.DepartmentId,
                JobTitleId = request.JobTitleId,
                Location = request.Location,
                EmployeeId = request.EmployeeId,
                Status = Core.Enums.RequestStatus.Approved,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _userService.CreateUserAsync(user, request.Password);
            
            var dto = (UserDto)request;
            dto.Id = user.Id;
            
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UserDto dto)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            user.Name = dto.Name;
            user.Email = dto.Email;
            user.RoleId = dto.RoleId;
            user.DepartmentId = dto.DepartmentId;
            user.JobTitleId = dto.JobTitleId;
            user.Location = dto.Location;
            user.EmployeeId = dto.EmployeeId;
            user.Status = (Core.Enums.RequestStatus)dto.Status;
            user.IsActive = dto.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _userService.UpdateUserAsync(user);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _userService.DeleteUserAsync(id);
            return NoContent();
        }
    }
}
