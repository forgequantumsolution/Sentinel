using Application.FormQuery;
using Core.Models;

namespace Application.Interfaces.Services
{
    public interface ICsvDataSourceService
    {
        /// <summary>
        /// Reads a CSV file and returns tabular results compatible with the graph pipeline.
        /// </summary>
        Task<FormQueryResult> ExecuteAsync(CsvSourceConfig config);

        /// <summary>
        /// Saves an uploaded CSV file and returns the stored file path.
        /// </summary>
        Task<string> UploadAsync(Stream fileStream, string fileName, Guid? organizationId);
    }
}
