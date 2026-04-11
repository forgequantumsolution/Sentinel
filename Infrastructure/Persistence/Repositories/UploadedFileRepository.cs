using Application.Interfaces.Persistence;
using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories
{
    public class UploadedFileRepository : IUploadedFileRepository
    {
        private readonly AppDbContext _context;

        public UploadedFileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UploadedFile?> GetByIdAsync(Guid id)
        {
            return await _context.UploadedFiles.FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task AddAsync(UploadedFile file)
        {
            await _context.UploadedFiles.AddAsync(file);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var file = await _context.UploadedFiles.FindAsync(id);
            if (file != null)
            {
                file.IsDeleted = true;
                file.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
