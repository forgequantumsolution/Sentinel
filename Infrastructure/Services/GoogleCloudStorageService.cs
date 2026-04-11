using System.Text;
using Application.Interfaces.Services;
using Application.Models;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class GoogleCloudStorageService : IFileStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public GoogleCloudStorageService(IOptions<FileStorageSettings> settings)
        {
            var config = settings.Value;

            if (string.IsNullOrWhiteSpace(config.GcpCredentialJson))
                throw new InvalidOperationException("GcpCredentialJson is required when using GoogleCloudStorage provider.");

            _bucketName = config.GcpBucket ?? "uploads";

            var credential = GoogleCredential.FromJson(config.GcpCredentialJson);
            _storageClient = StorageClient.Create(credential);
        }

        public async Task<string> UploadAsync(Stream fileStream, string folder, string fileName)
        {
            var storedName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
            var objectName = $"{folder}/{storedName}";

            await _storageClient.UploadObjectAsync(_bucketName, objectName, null, fileStream);

            return objectName;
        }

        public async Task<Stream> ReadAsync(string filePath)
        {
            var stream = new MemoryStream();
            await _storageClient.DownloadObjectAsync(_bucketName, filePath, stream);
            stream.Position = 0;
            return stream;
        }

        public async Task DeleteAsync(string filePath)
        {
            await _storageClient.DeleteObjectAsync(_bucketName, filePath);
        }
    }
}
