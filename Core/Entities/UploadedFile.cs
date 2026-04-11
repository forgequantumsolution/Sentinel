using System.ComponentModel.DataAnnotations;

namespace Core.Entities
{
    public class UploadedFile : TenantEntity
    {
        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FileType { get; set; } = string.Empty; // csv, xlsx, etc.

        [Required]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string StorageProvider { get; set; } = string.Empty; // Local, AzureBlob, CloudflareR2

        /// <summary>JSON snapshot of the storage settings used at upload time.</summary>
        public string? StorageConfigJson { get; set; }

        /// <summary>JSON-serialized list of column names parsed from the file.</summary>
        public string? ColumnsJson { get; set; }

        public long SizeBytes { get; set; }
    }
}
