using Application.DTOs;
using Core.Entities;

namespace Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(RegisterRequest request);
        Task<(User user, string token)> LoginAsync(LoginRequest request);
    }
}
