using Core.Entities;
using Application.Common.Pagination;

namespace Application.Interfaces.Persistence
{
    public interface IGraphConfigRepository
    {
        Task<GraphConfigEntity?> GetByIdAsync(Guid id);
        Task<GraphConfigEntity?> GetByNameAsync(string name);
        Task<PagedResult<GraphConfigEntity>> GetAllAsync(PageRequest pageRequest);
        Task<PagedResult<GraphConfigEntity>> GetByTypeAsync(Core.Enums.GraphType type, PageRequest pageRequest);
        Task AddAsync(GraphConfigEntity graphConfig);
        Task UpdateAsync(GraphConfigEntity graphConfig);
        Task DeleteAsync(Guid id);
    }
}