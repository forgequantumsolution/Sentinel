using Analytics_BE.Core.Entities;

namespace Analytics_BE.Application.Interfaces.Persistence
{
    public interface IDepartmentRepository
    {
        Task<Department?> GetByIdAsync(Guid id);
        Task<Department?> GetByNameAsync(string name);
        Task<List<Department>> GetAllAsync();
        Task AddAsync(Department department);
        Task UpdateAsync(Department department);
        Task DeleteAsync(Guid id);
    }
}
