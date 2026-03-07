using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Persistence;
using Core.Entities;
using Infrastructure.Persistence;
using Application.Common.Pagination;

namespace Infrastructure.Persistence.Repositories
{
    public class DynamicFormRepository : IDynamicFormRepository
    {
        private readonly AppDbContext _context;

        public DynamicFormRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DynamicForm?> GetByIdAsync(Guid id)
        {
            return await _context.DynamicForms
                .Include(f => f.FieldDefinitions)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task<PagedResult<DynamicForm>> GetAllAsync(PageRequest pageRequest)
        {
            var query = _context.DynamicForms.Where(f => !f.IsDeleted);
            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip(pageRequest.Skip)
                .Take(pageRequest.PageSize)
                .ToListAsync();

            return new PagedResult<DynamicForm>
            {
                Items = items,
                TotalCount = totalCount,
                Page = pageRequest.Page,
                PageSize = pageRequest.PageSize
            };
        }

        public async Task AddAsync(DynamicForm form)
        {
            await _context.DynamicForms.AddAsync(form);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DynamicForm form)
        {
            _context.DynamicForms.Update(form);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var form = await GetByIdAsync(id);
            if (form != null)
            {
                form.IsDeleted = true;
                form.IsActive = false;
                form.DeletedAt = DateTime.UtcNow;
                _context.DynamicForms.Update(form);
                await _context.SaveChangesAsync();
            }
        }
    }
}
