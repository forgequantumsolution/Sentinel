using Core.Entities;
using Application.Common.Pagination;

namespace Application.Interfaces.Persistence
{
    public interface IDynamicFormRepository
    {
        Task<DynamicForm?> GetByIdAsync(Guid id);
        Task<PagedResult<DynamicForm>> GetAllAsync(PageRequest pageRequest);
        Task AddAsync(DynamicForm form);
        Task UpdateAsync(DynamicForm form);
        Task DeleteAsync(Guid id);
    }
}
