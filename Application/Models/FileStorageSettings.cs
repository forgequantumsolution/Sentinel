namespace Application.Models
{
    public class FileStorageSettings
    {
        public const string SectionName = "FileStorage";

        /// <summary>"Local", "AzureBlob", "CloudflareR2", "AwsS3", or "GoogleCloudStorage"</summary>
        public string Provider { get; set; } = "Local";

        /// <summary>Root path for local file storage.</summary>
        public string LocalPath { get; set; } = "Uploads";

        /// <summary>Azure Blob Storage connection string.</summary>
        public string? AzureBlobConnectionString { get; set; }

        /// <summary>Azure Blob container name.</summary>
        public string? AzureBlobContainer { get; set; } = "uploads";

        /// <summary>Cloudflare account ID (used to build the R2 endpoint).</summary>
        public string? CloudflareAccountId { get; set; }

        /// <summary>R2 access key ID.</summary>
        public string? CloudflareAccessKeyId { get; set; }

        /// <summary>R2 secret access key.</summary>
        public string? CloudflareSecretAccessKey { get; set; }

        /// <summary>R2 bucket name.</summary>
        public string? CloudflareBucket { get; set; } = "uploads";

        /// <summary>AWS access key ID.</summary>
        public string? AwsAccessKeyId { get; set; }

        /// <summary>AWS secret access key.</summary>
        public string? AwsSecretAccessKey { get; set; }

        /// <summary>AWS region (e.g. us-east-1).</summary>
        public string? AwsRegion { get; set; }

        /// <summary>AWS S3 bucket name.</summary>
        public string? AwsBucket { get; set; } = "uploads";

        /// <summary>GCP service account JSON credential (the full JSON string or file path).</summary>
        public string? GcpCredentialJson { get; set; }

        /// <summary>GCP project ID.</summary>
        public string? GcpProjectId { get; set; }

        /// <summary>Google Cloud Storage bucket name.</summary>
        public string? GcpBucket { get; set; } = "uploads";
    }
}
