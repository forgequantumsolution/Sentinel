using Core.Entities;
using Application.Common.Pagination;

namespace Application.Interfaces.Persistence
{
    public interface IGraphConfigRepository
    {
        Task<GraphConfigEntity?> GetByIdAsync(Guid id);
        Task<GraphConfigEntity?> GetByNameAsync(string name);
        /// <summary>Returns graph configs only (ComponentType == null).</summary>
        Task<PagedResult<GraphConfigEntity>> GetAllAsync(PageRequest pageRequest);
        Task<PagedResult<GraphConfigEntity>> GetByTypeAsync(Core.Enums.GraphType type, PageRequest pageRequest);
        Task<List<GraphConfigEntity>> GetByActionObjectIdsAsync(List<Guid> actionObjectIds);
        /// <summary>Returns UI component configs for the given component type.</summary>
        Task<PagedResult<GraphConfigEntity>> GetByComponentTypeAsync(Core.Enums.UiComponentType componentType, PageRequest pageRequest);
        Task AddAsync(GraphConfigEntity graphConfig);
        Task UpdateAsync(GraphConfigEntity graphConfig);
        Task DeleteAsync(Guid id);
    }
}