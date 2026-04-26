using Application.Common.Pagination;
using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IDepartmentRepository
    {
        Task<Department?> GetByIdAsync(Guid id);
        Task<Department?> GetByNameAsync(string name);
        Task<PagedResult<Department>> GetAllAsync(PageRequest pageRequest);
        Task AddAsync(Department department);
        Task UpdateAsync(Department department);
        Task DeleteAsync(Guid id);
    }
}
