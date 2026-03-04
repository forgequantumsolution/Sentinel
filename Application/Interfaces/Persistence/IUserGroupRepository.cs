using Analytics_BE.Core.Entities;

namespace Analytics_BE.Application.Interfaces.Persistence
{
    public interface IUserGroupRepository
    {
        Task<UserGroup?> GetByIdAsync(Guid id);
        Task<List<UserGroup>> GetAllAsync();
        Task<List<UserGroup>> GetAllWithRulesAsync();
        Task AddAsync(UserGroup group);
        Task UpdateAsync(UserGroup group);
        Task DeleteAsync(Guid id);
    }
}
