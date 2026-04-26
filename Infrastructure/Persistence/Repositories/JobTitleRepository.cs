using Application.Common.Pagination;
using Core.Entities;
using Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
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

        public async Task<PagedResult<JobTitle>> GetAllAsync(PageRequest pageRequest)
        {
            var query = _context.JobTitles.OrderBy(j => j.Title);

            var totalCount = await query.CountAsync();
            var items = await query.Skip(pageRequest.Skip).Take(pageRequest.PageSize).ToListAsync();

            return new PagedResult<JobTitle>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
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
