using Application.Common.Pagination;
using Core.Entities;

namespace Application.Interfaces.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<PagedResult<User>> GetAllUsersAsync(PageRequest pageRequest);
        Task<User> CreateUserAsync(User user, string password);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(Guid userId);
        Task<bool> VerifyPasswordAsync(Guid userId, string password);
    }
}
