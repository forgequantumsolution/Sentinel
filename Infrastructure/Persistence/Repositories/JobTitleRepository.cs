using Analytics_BE.Core.Entities;
using Analytics_BE.Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Analytics_BE.Infrastructure.Persistence.Repositories
{
    public class JobTitleRepository : IJobTitleRepository
    {
        private readonly AppDbContext _context;

        public JobTitleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<JobTitle?> GetByIdAsync(Guid id)
        {
            return await _context.JobTitles.FindAsync(id);
        }

        public async Task<List<JobTitle>> GetAllAsync()
        {
            return await _context.JobTitles.ToListAsync();
        }

        public async Task AddAsync(JobTitle jobTitle)
        {
            await _context.JobTitles.AddAsync(jobTitle);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(JobTitle jobTitle)
        {
            _context.JobTitles.Update(jobTitle);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var jobTitle = await _context.JobTitles.FindAsync(id);
            if (jobTitle != null)
            {
                jobTitle.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
