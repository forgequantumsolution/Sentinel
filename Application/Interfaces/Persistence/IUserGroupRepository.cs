using Application.Common.Pagination;
using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IUserGroupRepository
    {
        Task<UserGroup?> GetByIdAsync(Guid id);
        Task<List<UserGroup>> GetAllAsync();
        Task<PagedResult<UserGroup>> GetAllAsync(PageRequest pageRequest);
        Task<List<UserGroup>> GetAllWithRulesAsync();
        Task AddAsync(UserGroup group);
        Task UpdateAsync(UserGroup group);
        Task DeleteAsync(Guid id);
    }
}
