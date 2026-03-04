using System;

namespace Analytics_BE.Application.DTOs
{
    public class DepartmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Code { get; set; }
        public Guid? ParentDepartmentId { get; set; }
        public bool IsActive { get; set; }
    }

    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
    }

    public class JobTitleDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? DepartmentId { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserGroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Type { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? RoleId { get; set; }
        public bool IsActive { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public Guid RoleId { get; set; }
        public string? RoleName { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
        public Guid? JobTitleId { get; set; }
        public string? JobTitle { get; set; }
        public string? Location { get; set; }
        public string? EmployeeId { get; set; }
        public int Status { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateUserRequest : UserDto
    {
        public string Password { get; set; } = string.Empty;
    }
}
