using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Persistence;
using Core.Entities;

namespace Infrastructure.Persistence.Repositories
{
    public class BulkUploadJobRepository : IBulkUploadJobRepository
    {
        private readonly AppDbContext _context;

        public BulkUploadJobRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BulkUploadJob?> GetByIdAsync(Guid id)
        {
            return await _context.BulkUploadJobs
                .FirstOrDefaultAsync(j => j.Id == id && !j.IsDeleted);
        }

        public async Task<IEnumerable<BulkUploadJob>> GetPendingJobsAsync()
        {
            return await _context.BulkUploadJobs
                .Where(j => j.Status == BulkUploadJobStatus.Pending && !j.IsDeleted)
                .OrderBy(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(BulkUploadJob job)
        {
            await _context.BulkUploadJobs.AddAsync(job);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(BulkUploadJob job)
        {
            _context.BulkUploadJobs.Update(job);
            await _context.SaveChangesAsync();
        }
    }
}
