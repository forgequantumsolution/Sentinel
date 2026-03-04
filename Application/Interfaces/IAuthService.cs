using Analytics_BE.Application.DTOs;
using Analytics_BE.Core.Entities;

namespace Analytics_BE.Application.Interfaces
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(RegisterRequest request);
        Task<(User user, string token)> LoginAsync(LoginRequest request);
    }
}
