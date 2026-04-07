using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IExcelParserService
    {
        /// <summary>
        /// Parses an Excel file into a BulkCreateDynamicFormDto.
        /// Each sheet becomes a form; row 1 headers become field definitions.
        /// </summary>
        Task<BulkCreateDynamicFormDto> ParseAsync(Stream excelStream);

        /// <summary>
        /// Generates an Excel file for a form with field definitions as column headers
        /// and optionally populated with submission data rows.
        /// </summary>
        Task<byte[]> GenerateExcelAsync(string formName, List<DynamicFormFieldDefinitionDto> fieldDefinitions, List<Dictionary<Guid, string?>>? rows = null);

        /// <summary>
        /// Generates a blank Excel template for bulk creating dynamic forms.
        /// </summary>
        byte[] GenerateBulkCreateTemplate();
    }
}
