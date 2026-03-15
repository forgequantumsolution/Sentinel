using Core.Entities;
using Core.Enums;

namespace Application.Interfaces.Persistence
{
    public interface IActionObjectRepository
    {
        Task<ActionObject?> GetByIdAsync(Guid id);
        Task<List<ActionObject>> GetAllAsync();
        Task<List<ActionObject>> GetByTypeAsync(ObjectType objectType);
        Task<List<ActionObject>> GetRootsByTypeAsync(ObjectType objectType);
        Task<List<ActionObject>> GetChildrenAsync(Guid parentObjectId);
        Task<ActionObject?> GetByRouteAsync(string route);
        Task<bool> RouteExistsAsync(string route, Guid? excludeId = null);
        Task AddAsync(ActionObject actionObject);
        Task UpdateAsync(ActionObject actionObject);
        Task DeleteAsync(Guid id);
    }
}
