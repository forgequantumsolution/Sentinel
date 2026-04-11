using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Application.Interfaces.Services;
using Application.Models;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    public class AwsS3StorageService : IFileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        public AwsS3StorageService(IOptions<FileStorageSettings> settings)
        {
            var config = settings.Value;

            if (string.IsNullOrWhiteSpace(config.AwsAccessKeyId) || string.IsNullOrWhiteSpace(config.AwsSecretAccessKey))
                throw new InvalidOperationException("AWS access key and secret are required when using AwsS3 provider.");

            if (string.IsNullOrWhiteSpace(config.AwsRegion))
                throw new InvalidOperationException("AwsRegion is required when using AwsS3 provider.");

            _bucketName = config.AwsBucket ?? "uploads";

            _s3Client = new AmazonS3Client(
                config.AwsAccessKeyId,
                config.AwsSecretAccessKey,
                RegionEndpoint.GetBySystemName(config.AwsRegion));
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
