using Application.FormQuery;
using Application.Interfaces.Services;
using Core.Models;

namespace Infrastructure.Services
{
    public class CsvDataSourceService : ICsvDataSourceService
    {
        private readonly IFileStorageService _fileStorage;

        public CsvDataSourceService(IFileStorageService fileStorage)
        {
            _fileStorage = fileStorage;
        }

        public async Task<FormQueryResult> ExecuteAsync(CsvSourceConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.FilePath))
                throw new ArgumentException("CSV file path is required.");

            await using var stream = await _fileStorage.ReadAsync(config.FilePath);
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
                //Columns = columns,
                Rows = rows,
                TotalCount = rows.Count
            };
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, Guid? organizationId)
        {
            var folder = $"csv/{organizationId?.ToString() ?? "shared"}";
            return await _fileStorage.UploadAsync(fileStream, folder, fileName);
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
