namespace Application.Models
{
    public class FileStorageSettings
    {
        public const string SectionName = "FileStorage";

        /// <summary>"Local", "AzureBlob", or "CloudflareR2"</summary>
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
    }
}
