using Application.Interfaces.Services;
using Application.Models;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _rootPath;

        public LocalFileStorageService(IOptions<FileStorageSettings> settings)
        {
            _rootPath = settings.Value.LocalPath;
        }

        public async Task<string> UploadAsync(Stream fileStream, string folder, string fileName)
        {
            var directory = Path.Combine(_rootPath, folder);
            Directory.CreateDirectory(directory);

            var storedName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
            var filePath = Path.Combine(directory, storedName);

            await using var fs = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fs);

            return filePath;
        }

        public Task<Stream> ReadAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            return Task.FromResult(stream);
        }

        public Task DeleteAsync(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            return Task.CompletedTask;
        }
    }
}
