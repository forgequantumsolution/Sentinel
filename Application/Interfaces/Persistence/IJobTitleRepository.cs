using Application.Common.Pagination;
using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IJobTitleRepository
    {
        Task<JobTitle?> GetByIdAsync(Guid id);
        Task<PagedResult<JobTitle>> GetAllAsync(PageRequest pageRequest);
        Task AddAsync(JobTitle jobTitle);
        Task UpdateAsync(JobTitle jobTitle);
        Task DeleteAsync(Guid id);
    }
}
