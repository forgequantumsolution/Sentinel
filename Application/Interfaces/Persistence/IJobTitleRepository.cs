using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IJobTitleRepository
    {
        Task<JobTitle?> GetByIdAsync(Guid id);
        Task<List<JobTitle>> GetAllAsync();
        Task AddAsync(JobTitle jobTitle);
        Task UpdateAsync(JobTitle jobTitle);
        Task DeleteAsync(Guid id);
    }
}
