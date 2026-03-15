using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IAppFolderService
    {
        Task<AppFolderDto?> GetByIdAsync(Guid id);
        Task<List<AppFolderDto>> GetAllTreeAsync();
        Task<AppFolderDto?> GetByRouteAsync(string route);
        Task<AppFolderDto> CreateAsync(CreateAppFolderDto dto, Guid? userId);
        Task<AppFolderDto?> UpdateAsync(Guid id, UpdateAppFolderDto dto);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> MoveToFolderAsync(Guid folderId, Guid actionObjectId);
        Task<bool> RemoveFromFolderAsync(Guid actionObjectId);
    }
}
