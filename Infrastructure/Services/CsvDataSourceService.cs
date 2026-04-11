using System.Text.Json;
using Application.FormQuery;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Application.Models;
using Core.Entities;
using Core.Models;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

namespace Infrastructure.Services
{
    public class CsvDataSourceService : ICsvDataSourceService
    {
        private readonly IFileStorageService _defaultFileStorage;
        private readonly IUploadedFileRepository _uploadedFileRepository;
        private readonly IEncryptionService _encryption;
        private readonly FileStorageSettings _storageSettings;

        public CsvDataSourceService(
            IFileStorageService fileStorage,
            IUploadedFileRepository uploadedFileRepository,
            IEncryptionService encryption,
            IOptions<FileStorageSettings> storageSettings)
        {
            _defaultFileStorage = fileStorage;
            _uploadedFileRepository = uploadedFileRepository;
            _encryption = encryption;
            _storageSettings = storageSettings.Value;
        }

        public async Task<FormQueryResult> ExecuteAsync(CsvSourceConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.FilePath))
                throw new ArgumentException("File ID is required.");

            if (!Guid.TryParse(config.FilePath, out var fileId))
                throw new ArgumentException("Invalid file ID.");

            var uploadedFile = await _uploadedFileRepository.GetByIdAsync(fileId)
                ?? throw new KeyNotFoundException($"Uploaded file with ID {fileId} not found.");

            var storage = ResolveStorageProvider(uploadedFile);

            var ext = Path.GetExtension(uploadedFile.FileName)?.ToLowerInvariant();
            return ext is ".xlsx" or ".xls"
                ? await ExecuteExcelAsync(storage, uploadedFile.FilePath, config)
                : await ExecuteCsvAsync(storage, uploadedFile.FilePath, config);
        }

        private async Task<FormQueryResult> ExecuteCsvAsync(IFileStorageService storage, string storedPath, CsvSourceConfig config)
        {
            await using var stream = await storage.ReadAsync(storedPath);
            using var reader = new StreamReader(stream);
            var content = await reader.ReadToEndAsync();
            var lines = content.Split('\n', StringSplitOptions.None);

            if (lines.Length == 0)
                return new FormQueryResult();

            var delimiter = config.Delimiter;
            var startIndex = 0;
            var columns = new List<string>();

            if (config.HasHeader)
            {
                columns = ParseCsvLine(lines[0], delimiter);
                startIndex = 1;
            }
            else
            {
                var firstRow = ParseCsvLine(lines[0], delimiter);
                columns = Enumerable.Range(0, firstRow.Count).Select(i => $"Column{i}").ToList();
            }

            var rows = new List<Dictionary<string, object?>>();

            for (var i = startIndex; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var values = ParseCsvLine(line, delimiter);
                var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

                for (var j = 0; j < columns.Count; j++)
                {
                    var raw = j < values.Count ? values[j] : null;
                    row[columns[j]] = ParseValue(raw);
                }

                rows.Add(row);
            }

            return new FormQueryResult
            {
                Rows = rows,
                TotalCount = rows.Count
            };
        }

        private async Task<FormQueryResult> ExecuteExcelAsync(IFileStorageService storage, string storedPath, CsvSourceConfig config)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            await using var stream = await storage.ReadAsync(storedPath);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Position = 0;
            using var package = new ExcelPackage(ms);

            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet?.Dimension == null)
                return new FormQueryResult();

            var rowCount = worksheet.Dimension.Rows;
            var colCount = worksheet.Dimension.Columns;
            var startRow = config.HasHeader ? 2 : 1;

            var columns = new List<string>();
            if (config.HasHeader)
            {
                for (var col = 1; col <= colCount; col++)
                {
                    var header = worksheet.Cells[1, col].Text?.Trim();
                    columns.Add(string.IsNullOrWhiteSpace(header) ? $"Column{col}" : header);
                }
            }
            else
            {
                columns = Enumerable.Range(0, colCount).Select(i => $"Column{i}").ToList();
            }

            var rows = new List<Dictionary<string, object?>>();

            for (var row = startRow; row <= rowCount; row++)
            {
                var rowData = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
                var isEmpty = true;

                for (var col = 1; col <= colCount; col++)
                {
                    var cellValue = worksheet.Cells[row, col].Value;
                    if (cellValue != null) isEmpty = false;
                    rowData[columns[col - 1]] = cellValue;
                }

                if (!isEmpty) rows.Add(rowData);
            }

            return new FormQueryResult
            {
                Rows = rows,
                TotalCount = rows.Count
            };
        }

        /// <summary>
        /// Resolves the correct storage provider from the uploaded file's stored provider name and config.
        /// Falls back to the default (current) provider if the stored config can't be deserialized.
        /// </summary>
        private IFileStorageService ResolveStorageProvider(UploadedFile uploadedFile)
        {
            if (string.IsNullOrWhiteSpace(uploadedFile.StorageConfigJson)
                || string.IsNullOrWhiteSpace(uploadedFile.StorageProvider))
                return _defaultFileStorage;

            // If the stored provider matches the current default, reuse it
            if (uploadedFile.StorageProvider.Equals(_storageSettings.Provider, StringComparison.OrdinalIgnoreCase))
                return _defaultFileStorage;

            // Decrypt and rebuild settings from the snapshot stored at upload time
            var decryptedJson = _encryption.Decrypt(uploadedFile.StorageConfigJson);
            var storedSettings = JsonSerializer.Deserialize<FileStorageSettings>(
                decryptedJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (storedSettings == null)
                return _defaultFileStorage;

            storedSettings.Provider = uploadedFile.StorageProvider;
            var options = Options.Create(storedSettings);

            return uploadedFile.StorageProvider.ToLowerInvariant() switch
            {
                "azureblob" => new AzureBlobStorageService(options),
                "cloudflarer2" => new CloudflareR2StorageService(options),
                "awss3" => new AwsS3StorageService(options),
                "googlecloudstorage" => new GoogleCloudStorageService(options),
                _ => new LocalFileStorageService(options)
            };
        }

        public async Task<CsvUploadResult> UploadAsync(Stream fileStream, string fileName, Guid? organizationId)
        {
            // Buffer the upload so we can both store it and parse headers
            using var memory = new MemoryStream();
            await fileStream.CopyToAsync(memory);
            var fileBytes = memory.ToArray();

            // Upload to storage
            using var uploadStream = new MemoryStream(fileBytes);
            var folder = $"csv/{organizationId?.ToString() ?? "shared"}";
            var storedPath = await _defaultFileStorage.UploadAsync(uploadStream, folder, fileName);

            // Parse column headers based on file type
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            var columns = ext is ".xlsx" or ".xls"
                ? ParseExcelHeaderColumns(fileBytes)
                : ParseCsvHeaderColumns(fileBytes);

            // Snapshot storage config (excluding sensitive secrets)
            var storageConfigSnapshot = _encryption.Encrypt(JsonSerializer.Serialize(_storageSettings));

            // Persist metadata
            var entity = new UploadedFile
            {
                FileName = fileName,
                FileType = ext?.TrimStart('.') ?? "csv",
                FilePath = storedPath,
                StorageProvider = _storageSettings.Provider,
                StorageConfigJson = storageConfigSnapshot,
                ColumnsJson = JsonSerializer.Serialize(columns),
                SizeBytes = fileBytes.LongLength,
                OrganizationId = organizationId,
                CreatedAt = DateTime.UtcNow
            };

            await _uploadedFileRepository.AddAsync(entity);

            return new CsvUploadResult
            {
                FileId = entity.Id,
                Columns = columns
            };
        }

        private static List<string> ParseCsvHeaderColumns(byte[] fileBytes)
        {
            using var ms = new MemoryStream(fileBytes);
            using var reader = new StreamReader(ms);
            var firstLine = reader.ReadLine();
            return string.IsNullOrWhiteSpace(firstLine)
                ? new List<string>()
                : ParseCsvLine(firstLine, ',');
        }

        private static List<string> ParseExcelHeaderColumns(byte[] fileBytes)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var ms = new MemoryStream(fileBytes);
            using var package = new ExcelPackage(ms);

            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet?.Dimension == null) return new List<string>();

            var columns = new List<string>();
            for (var col = 1; col <= worksheet.Dimension.Columns; col++)
            {
                var header = worksheet.Cells[1, col].Text?.Trim();
                columns.Add(string.IsNullOrWhiteSpace(header) ? $"Column{col}" : header);
            }
            return columns;
        }

        private static List<string> ParseCsvLine(string line, char delimiter)
        {
            var fields = new List<string>();
            var current = "";
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            current += '"';
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        current += c;
                    }
                }
                else if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == delimiter)
                {
                    fields.Add(current.Trim());
                    current = "";
                }
                else
                {
                    current += c;
                }
            }

            fields.Add(current.Trim());
            return fields;
        }

        private static object? ParseValue(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            if (long.TryParse(raw, out var longVal)) return longVal;
            if (decimal.TryParse(raw, out var decVal)) return decVal;
            if (DateTime.TryParse(raw, out var dateVal)) return dateVal;
            if (bool.TryParse(raw, out var boolVal)) return boolVal;

            return raw;
        }
    }
}
