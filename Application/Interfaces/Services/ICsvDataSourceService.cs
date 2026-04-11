using Application.FormQuery;
using Core.Models;

namespace Application.Interfaces.Services
{
    public class CsvUploadResult
    {
        public Guid FileId { get; set; }
        public List<string> Columns { get; set; } = new();
    }

    public interface ICsvDataSourceService
    {
        /// <summary>
        /// Reads a CSV file and returns tabular results compatible with the graph pipeline.
        /// </summary>
        Task<FormQueryResult> ExecuteAsync(CsvSourceConfig config);

        /// <summary>
        /// Uploads a CSV file, parses its column headers, persists file metadata,
        /// and returns the new file id and the parsed column names.
        /// </summary>
        Task<CsvUploadResult> UploadAsync(Stream fileStream, string fileName, Guid? organizationId);
    }
}
