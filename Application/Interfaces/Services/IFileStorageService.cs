namespace Application.Interfaces.Services
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Uploads a file and returns the stored path/key.
        /// </summary>
        Task<string> UploadAsync(Stream fileStream, string folder, string fileName);

        /// <summary>
        /// Opens a file for reading by its stored path/key.
        /// </summary>
        Task<Stream> ReadAsync(string filePath);

        /// <summary>
        /// Deletes a file by its stored path/key.
        /// </summary>
        Task DeleteAsync(string filePath);
    }
}
