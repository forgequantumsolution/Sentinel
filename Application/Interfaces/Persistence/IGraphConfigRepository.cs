using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IGraphConfigRepository
    {
        Task<GraphConfigEntity?> GetByIdAsync(Guid id);
        Task<GraphConfigEntity?> GetByNameAsync(string name);
        Task<List<GraphConfigEntity>> GetAllAsync();
        Task<List<GraphConfigEntity>> GetByTypeAsync(Core.Enums.GraphType type);
        Task AddAsync(GraphConfigEntity graphConfig);
        Task UpdateAsync(GraphConfigEntity graphConfig);
        Task DeleteAsync(Guid id);
    }
}