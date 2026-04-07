using Application.Interfaces.Services;
using Application.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly BlobContainerClient _containerClient;

        public AzureBlobStorageService(IOptions<FileStorageSettings> settings)
        {
            var config = settings.Value;
            if (string.IsNullOrWhiteSpace(config.AzureBlobConnectionString))
                throw new InvalidOperationException("AzureBlobConnectionString is required when using AzureBlob provider.");

            var serviceClient = new BlobServiceClient(config.AzureBlobConnectionString);
            _containerClient = serviceClient.GetBlobContainerClient(config.AzureBlobContainer ?? "uploads");
            _containerClient.CreateIfNotExists();
        }

        public async Task<string> UploadAsync(Stream fileStream, string folder, string fileName)
        {
            var storedName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
            var blobPath = $"{folder}/{storedName}";

            var blobClient = _containerClient.GetBlobClient(blobPath);
            await blobClient.UploadAsync(fileStream, overwrite: true);

            return blobPath;
        }

        public async Task<Stream> ReadAsync(string filePath)
        {
            var blobClient = _containerClient.GetBlobClient(filePath);

            if (!await blobClient.ExistsAsync())
                throw new FileNotFoundException($"Blob not found: {filePath}");

            var stream = new MemoryStream();
            await blobClient.DownloadToAsync(stream);
            stream.Position = 0;
            return stream;
        }

        public async Task DeleteAsync(string filePath)
        {
            var blobClient = _containerClient.GetBlobClient(filePath);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
