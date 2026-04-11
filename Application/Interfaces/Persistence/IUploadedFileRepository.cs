using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IUploadedFileRepository
    {
        Task<UploadedFile?> GetByIdAsync(Guid id);
        Task AddAsync(UploadedFile file);
        Task DeleteAsync(Guid id);
    }
}
