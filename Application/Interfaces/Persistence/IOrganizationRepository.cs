using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IOrganizationRepository
    {
        Task<Organization?> GetByIdAsync(Guid id);
        Task<Organization?> GetByCodeAsync(string code);
        Task<List<Organization>> GetAllAsync();
        Task AddAsync(Organization organization);
        Task UpdateAsync(Organization organization);
        Task DeleteAsync(Guid id);
    }
}
