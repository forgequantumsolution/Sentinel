using Amazon.S3;
using Amazon.S3.Model;
using Application.Interfaces.Services;
using Application.Models;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class CloudflareR2StorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public CloudflareR2StorageService(IOptions<FileStorageSettings> settings)
        {
            var config = settings.Value;

            if (string.IsNullOrWhiteSpace(config.CloudflareAccountId))
                throw new InvalidOperationException("CloudflareAccountId is required when using CloudflareR2 provider.");
            if (string.IsNullOrWhiteSpace(config.CloudflareAccessKeyId) || string.IsNullOrWhiteSpace(config.CloudflareSecretAccessKey))
                throw new InvalidOperationException("Cloudflare R2 access key and secret are required.");

            _bucketName = config.CloudflareBucket ?? "uploads";

            _s3Client = new AmazonS3Client(
                config.CloudflareAccessKeyId,
                config.CloudflareSecretAccessKey,
                new AmazonS3Config
                {
                    ServiceURL = $"https://{config.CloudflareAccountId}.r2.cloudflarestorage.com",
                    ForcePathStyle = true
                });
        }

        public async Task<string> UploadAsync(Stream fileStream, string folder, string fileName)
        {
            var storedName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
            var key = $"{folder}/{storedName}";

            await _s3Client.PutObjectAsync(new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = fileStream
            });

            return key;
        }

        public async Task<Stream> ReadAsync(string filePath)
        {
            var response = await _s3Client.GetObjectAsync(new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = filePath
            });

            var stream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(stream);
            stream.Position = 0;
            return stream;
        }

        public async Task DeleteAsync(string filePath)
        {
            await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = filePath
            });
        }
    }
}
