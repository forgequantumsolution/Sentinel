using Application.Common.Pagination;
using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(Guid id);
        Task<Role?> GetByNameAsync(string name);
        Task<PagedResult<Role>> GetAllAsync(PageRequest pageRequest);
        Task AddAsync(Role role);
        Task UpdateAsync(Role role);
        Task DeleteAsync(Guid id);
    }
}
