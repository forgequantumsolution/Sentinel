using Core.Entities;
using Core.Enums;
using Application.Common.Pagination;

namespace Application.Interfaces.Persistence
{
    public interface IActionObjectRepository
    {
        Task<ActionObject?> GetByIdAsync(Guid id);
        Task<List<ActionObject>> GetAllAsync();
        Task<PagedResult<ActionObject>> GetByTypeAsync(ObjectType objectType, PageRequest pageRequest);
        Task<List<ActionObject>> GetRootsByTypeAsync(ObjectType objectType);
        Task<List<ActionObject>> GetChildrenAsync(Guid parentObjectId);
        Task<PagedResult<ActionObject>> GetChildrenPagedAsync(Guid parentObjectId, PageRequest pageRequest);
        Task<ActionObject?> GetByRouteAsync(string route);
        Task<bool> RouteExistsAsync(string route, Guid? excludeId = null);
        Task AddAsync(ActionObject actionObject);
        Task UpdateAsync(ActionObject actionObject);
        Task DeleteAsync(Guid id);
    }
}
