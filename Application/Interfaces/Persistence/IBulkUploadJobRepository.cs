using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IBulkUploadJobRepository
    {
        Task<BulkUploadJob?> GetByIdAsync(Guid id);
        Task<IEnumerable<BulkUploadJob>> GetPendingJobsAsync();
        Task AddAsync(BulkUploadJob job);
        Task UpdateAsync(BulkUploadJob job);
    }
}
