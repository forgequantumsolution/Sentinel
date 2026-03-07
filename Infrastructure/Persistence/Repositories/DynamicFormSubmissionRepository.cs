using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Persistence;
using Core.Entities;
using Infrastructure.Persistence;
using Application.Common.Pagination;

namespace Infrastructure.Persistence.Repositories
{
    public class DynamicFormSubmissionRepository : IDynamicFormSubmissionRepository
    {
        private readonly AppDbContext _context;

        public DynamicFormSubmissionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DynamicFormSubmission?> GetByIdAsync(Guid id)
        {
            return await _context.DynamicFormSubmissions.FindAsync(id);
        }

        public async Task<PagedResult<DynamicFormSubmission>> GetByFormIdAsync(Guid formId, PageRequest pageRequest)
        {
            var query = _context.DynamicFormSubmissions.Where(s => s.FormId == formId);
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip(pageRequest.Skip)
                .Take(pageRequest.PageSize)
                .ToListAsync();

            return new PagedResult<DynamicFormSubmission>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }

        public async Task AddAsync(DynamicFormSubmission submission)
        {
            await _context.DynamicFormSubmissions.AddAsync(submission);
            await _context.SaveChangesAsync();
        }
    }
}
