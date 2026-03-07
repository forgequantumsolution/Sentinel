using Core.Entities;
using Core.Enums;
using Application.DTOs;
using Application.Interfaces;
using Application.Interfaces.Persistence;
using Microsoft.Extensions.Logging;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository, 
            IRoleRepository roleRepository,
            IDepartmentRepository departmentRepository,
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _departmentRepository = departmentRepository;
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<User> RegisterAsync(RegisterRequest request)
        {
            _logger.LogInformation("Attempting to register user with email: {Email}", request.Email);

            if (await _userRepository.ExistsAsync(request.Email))
            {
                _logger.LogWarning("Registration failed: User with email {Email} already exists", request.Email);
                throw new Exception("User with this email already exists");
            }

            var role = await _roleRepository.GetByNameAsync(request.Role ?? "user");
            if (role == null)
            {
                _logger.LogError("Registration failed: Role {Role} does not exist", request.Role);
                throw new Exception("Role does not exist");
            }

            var department = request.Department != null 
                ? await _departmentRepository.GetByNameAsync(request.Department) 
                : null;

            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                RoleId = role.Id,
                DepartmentId = department?.Id,
                PasswordHash = _passwordHasher.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                Status = RequestStatus.Approved
            };

            await _userRepository.AddAsync(user);
            _logger.LogInformation("User registration successful for email: {Email}", request.Email);
            return user;
        }

        public async Task<(User user, string token)> LoginAsync(LoginRequest request)
        {
            _logger.LogInformation("Login attempt for email: {Email}", request.Email);

            var user = await _userRepository.GetByEmailAsync(request.Email);

            if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid credentials for email {Email}", request.Email);
                throw new Exception("Invalid email or password");
            }

            if (user.Status != RequestStatus.Approved)
            {
                _logger.LogWarning("Login blocked: User {Email} is not authorized (Status: {Status})", request.Email, user.Status);
                throw new Exception("Your registration is not authorized");
            }

            var token = _tokenService.GenerateJwtToken(user);
            _logger.LogInformation("Login successful for email: {Email}", request.Email);
            return (user, token);
        }
    }
}
