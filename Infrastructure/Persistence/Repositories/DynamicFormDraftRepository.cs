using Microsoft.EntityFrameworkCore;
using Application.Interfaces.Persistence;
using Core.Entities;

namespace Infrastructure.Persistence.Repositories
{
    public class DynamicFormDraftRepository : IDynamicFormDraftRepository
    {
        private readonly AppDbContext _context;

        public DynamicFormDraftRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DynamicFormDraft?> GetByFormAndUserAsync(Guid formId, Guid userId)
        {
            return await _context.DynamicFormDrafts
                .FirstOrDefaultAsync(d => d.FormId == formId && d.CreatedById == userId);
        }

        public async Task AddAsync(DynamicFormDraft draft)
        {
            await _context.DynamicFormDrafts.AddAsync(draft);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DynamicFormDraft draft)
        {
            _context.DynamicFormDrafts.Update(draft);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var draft = await _context.DynamicFormDrafts.FindAsync(id);
            if (draft != null)
            {
                draft.IsDeleted = true;
                draft.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
