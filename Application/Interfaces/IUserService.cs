using Analytics_BE.Core.Entities;

namespace Analytics_BE.Application.Interfaces
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<List<User>> GetAllUsersAsync();
        Task<User> CreateUserAsync(User user, string password);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(Guid userId);
        Task<bool> VerifyPasswordAsync(Guid userId, string password);
    }
}
