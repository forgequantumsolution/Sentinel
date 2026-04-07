using System.Text.Json;
using Application.DTOs;
using Application.Interfaces.Services;
using OfficeOpenXml;

namespace Infrastructure.Services
{
    public class ExcelParserService : IExcelParserService
    {
        public Task<BulkCreateDynamicFormDto> ParseAsync(Stream excelStream)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage(excelStream);

            // Detect structured template: has "Forms", "Sections", "Fields" sheets
            var formsSheet = package.Workbook.Worksheets["Forms"];
            var sectionsSheet = package.Workbook.Worksheets["Sections"];
            var fieldsSheet = package.Workbook.Worksheets["Fields"];

            if (formsSheet != null && fieldsSheet != null)
                return Task.FromResult(ParseTemplateFormat(formsSheet, sectionsSheet, fieldsSheet));

            return Task.FromResult(ParseSheetPerFormFormat(package));
        }

        // ── Structured Template Parsing (Forms + Sections + Fields) ──

        private static BulkCreateDynamicFormDto ParseTemplateFormat(
            ExcelWorksheet formsSheet,
            ExcelWorksheet? sectionsSheet,
            ExcelWorksheet fieldsSheet)
        {
            var result = new BulkCreateDynamicFormDto();

            // 1. Parse sections grouped by form name
            // Key: FormName → list of { SectionName, Description, Order }
            var sectionsByForm = new Dictionary<string, List<SectionRow>>(StringComparer.OrdinalIgnoreCase);
            if (sectionsSheet?.Dimension != null)
            {
                for (var row = 2; row <= sectionsSheet.Dimension.Rows; row++)
                {
                    var formName = sectionsSheet.Cells[row, 1].Text?.Trim();
                    if (string.IsNullOrWhiteSpace(formName)) continue;

                    if (!sectionsByForm.ContainsKey(formName))
                        sectionsByForm[formName] = new List<SectionRow>();

                    sectionsByForm[formName].Add(new SectionRow
                    {
                        Name = sectionsSheet.Cells[row, 2].Text?.Trim() ?? "",
                        Description = sectionsSheet.Cells[row, 3].Text?.Trim(),
                        Order = int.TryParse(sectionsSheet.Cells[row, 4].Text?.Trim(), out var o) ? o : 0
                    });
                }
            }

            // 2. Parse fields grouped by (FormName, SectionName)
            // Key: "FormName||SectionName" → list of field rows
            var fieldsByKey = new Dictionary<string, List<FieldRow>>(StringComparer.OrdinalIgnoreCase);
            var flatFieldsByForm = new Dictionary<string, List<CreateDynamicFormFieldDefinitionDto>>(StringComparer.OrdinalIgnoreCase);

            if (fieldsSheet.Dimension != null)
            {
                for (var row = 2; row <= fieldsSheet.Dimension.Rows; row++)
                {
                    var formName = fieldsSheet.Cells[row, 1].Text?.Trim();
                    if (string.IsNullOrWhiteSpace(formName)) continue;

                    var sectionName = fieldsSheet.Cells[row, 2].Text?.Trim() ?? "";
                    var fieldName = fieldsSheet.Cells[row, 3].Text?.Trim() ?? "";
                    var fieldId = fieldsSheet.Cells[row, 4].Text?.Trim();
                    var fieldType = fieldsSheet.Cells[row, 5].Text?.Trim() ?? "String";
                    var isRequired = bool.TryParse(fieldsSheet.Cells[row, 6].Text?.Trim(), out var req) && req;
                    var validationRules = fieldsSheet.Cells[row, 7].Text?.Trim();
                    var order = int.TryParse(fieldsSheet.Cells[row, 8].Text?.Trim(), out var fo) ? fo : 0;

                    if (string.IsNullOrWhiteSpace(fieldId))
                        fieldId = $"field_{Guid.NewGuid():N}"[..30];

                    var key = $"{formName}||{sectionName}";
                    if (!fieldsByKey.ContainsKey(key))
                        fieldsByKey[key] = new List<FieldRow>();

                    fieldsByKey[key].Add(new FieldRow
                    {
                        FieldName = fieldName,
                        FieldId = fieldId,
                        FieldType = fieldType,
                        IsRequired = isRequired,
                        ValidationRules = validationRules,
                        Order = order
                    });

                    // Also collect flat field definitions for the DB
                    if (!flatFieldsByForm.ContainsKey(formName))
                        flatFieldsByForm[formName] = new List<CreateDynamicFormFieldDefinitionDto>();

                    flatFieldsByForm[formName].Add(new CreateDynamicFormFieldDefinitionDto
                    {
                        FieldName = fieldName,
                        FieldId = fieldId,
                        FieldType = fieldType,
                        IsRequired = isRequired,
                        ValidationRules = validationRules
                    });
                }
            }

            // 3. Parse forms and build ConfigJson
            if (formsSheet.Dimension != null)
            {
                for (var row = 2; row <= formsSheet.Dimension.Rows; row++)
                {
                    var name = formsSheet.Cells[row, 1].Text?.Trim();
                    if (string.IsNullOrWhiteSpace(name)) continue;

                    var description = formsSheet.Cells[row, 2].Text?.Trim() ?? "";
                    var isActive = !bool.TryParse(formsSheet.Cells[row, 3].Text?.Trim(), out var active) || active;

                    // Build ConfigJson from sections + fields in FE-compatible format
                    var configJson = BuildConfigJson(name, description, sectionsByForm, fieldsByKey);
                    flatFieldsByForm.TryGetValue(name, out var fieldDefs);

                    result.Forms.Add(new CreateDynamicFormDto
                    {
                        Name = name,
                        Description = description,
                        ConfigJson = configJson,
                        IsActive = isActive,
                        FieldDefinitions = fieldDefs
                    });
                }
            }

            return result;
        }

        private static string BuildConfigJson(
            string formName,
            string description,
            Dictionary<string, List<SectionRow>> sectionsByForm,
            Dictionary<string, List<FieldRow>> fieldsByKey)
        {
            sectionsByForm.TryGetValue(formName, out var sections);
            sections ??= new List<SectionRow> { new() { Name = "Basic Details", Order = 1 } };

            var emptyDependency = new
            {
                field_name = "",
                field_section = "",
                options_selected = Array.Empty<string>(),
                cascader_selection = Array.Empty<string>(),
                multiple_field_dependencies = Array.Empty<string>()
            };

            var sectionList = sections.OrderBy(s => s.Order).Select(s =>
            {
                var key = $"{formName}||{s.Name}";
                fieldsByKey.TryGetValue(key, out var sectionFields);

                return new
                {
                    section_id = Guid.NewGuid().ToString(),
                    section_name = s.Name,
                    dependency = emptyDependency,
                    fields = (sectionFields ?? new List<FieldRow>())
                        .OrderBy(f => f.Order)
                        .Select((f, idx) => new
                        {
                            field_id = Guid.NewGuid().ToString(),
                            label = f.FieldName,
                            name = f.FieldId,
                            type = MapFieldTypeToFeType(f.FieldType),
                            required = f.IsRequired,
                            dependency = emptyDependency,
                            dynamic = false,
                            end_point = (string?)null,
                            value = (object?)null,
                            width = "33",
                            options = Array.Empty<string>(),
                            position = idx,
                            validation = new { }
                        })
                        .ToArray()
                };
            }).ToArray();

            var config = new
            {
                form_details = new
                {
                    title = formName,
                    form_type = "Now",
                    description,
                    version = 1,
                    created_on = DateTime.UtcNow.ToString("o"),
                    workflow_name = (string?)null
                },
                sections = sectionList
            };

            return JsonSerializer.Serialize(config);
        }

        /// <summary>
        /// Maps our DB FieldType (String, Int, Decimal, DateTime, Boolean) to FE field types.
        /// </summary>
        private static string MapFieldTypeToFeType(string dbFieldType)
        {
            return dbFieldType?.Trim().ToLowerInvariant() switch
            {
                "string" => "text",
                "int" => "number",
                "decimal" => "number",
                "datetime" => "date",
                "date" => "date",
                "time" => "time",
                "boolean" => "checkbox",
                _ => "text"
            };
        }

        // ── Template Generator ──

        public byte[] GenerateBulkCreateTemplate()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            const int maxRows = 100; // validation range for dropdowns

            // Sheet 1: Forms
            var formsSheet = package.Workbook.Worksheets.Add("Forms");
            WriteHeaders(formsSheet, "Name", "Description", "IsActive");
            formsSheet.Cells[2, 1].Value = "Employee Form";
            formsSheet.Cells[2, 2].Value = "Employee data collection form";
            formsSheet.Cells[2, 3].Value = "true";
            // IsActive dropdown
            AddListValidation(formsSheet, 2, maxRows, 3, "true,false");
            formsSheet.Cells[formsSheet.Dimension.Address].AutoFitColumns();

            // Sheet 2: Sections
            var sectionsSheet = package.Workbook.Worksheets.Add("Sections");
            WriteHeaders(sectionsSheet, "FormName", "SectionName", "Description", "Order");
            sectionsSheet.Cells[2, 1].Value = "Employee Form";
            sectionsSheet.Cells[2, 2].Value = "Personal Info";
            sectionsSheet.Cells[2, 3].Value = "Basic personal details";
            sectionsSheet.Cells[2, 4].Value = 1;
            sectionsSheet.Cells[3, 1].Value = "Employee Form";
            sectionsSheet.Cells[3, 2].Value = "Employment Details";
            sectionsSheet.Cells[3, 3].Value = "Job related information";
            sectionsSheet.Cells[3, 4].Value = 2;
            // FormName dropdown → references Forms.Name column
            AddSheetReferenceValidation(sectionsSheet, 2, maxRows, 1, "Forms", "$A$2:$A$" + maxRows);
            sectionsSheet.Cells[sectionsSheet.Dimension.Address].AutoFitColumns();

            // Sheet 3: Fields
            var fieldsSheet = package.Workbook.Worksheets.Add("Fields");
            WriteHeaders(fieldsSheet, "FormName", "SectionName", "FieldName", "FieldId", "FieldType", "IsRequired", "ValidationRules", "Order");
            WriteFieldSample(fieldsSheet, 2, "Employee Form", "Personal Info", "First Name", "field_first_name", "String", "true", "", 1);
            WriteFieldSample(fieldsSheet, 3, "Employee Form", "Personal Info", "Last Name", "field_last_name", "String", "true", "", 2);
            WriteFieldSample(fieldsSheet, 4, "Employee Form", "Personal Info", "Date of Birth", "field_dob", "DateTime", "false", "", 3);
            WriteFieldSample(fieldsSheet, 5, "Employee Form", "Employment Details", "Job Title", "field_job_title", "String", "true", "", 1);
            WriteFieldSample(fieldsSheet, 6, "Employee Form", "Employment Details", "Salary", "field_salary", "Decimal", "false", "", 2);
            // FormName dropdown → references Forms.Name column
            AddSheetReferenceValidation(fieldsSheet, 2, maxRows, 1, "Forms", "$A$2:$A$" + maxRows);
            // SectionName dropdown → references Sections.SectionName column
            AddSheetReferenceValidation(fieldsSheet, 2, maxRows, 2, "Sections", "$B$2:$B$" + maxRows);
            // FieldType dropdown
            AddListValidation(fieldsSheet, 2, maxRows, 5, "String,Int,Decimal,DateTime,Boolean");
            // IsRequired dropdown
            AddListValidation(fieldsSheet, 2, maxRows, 6, "true,false");
            fieldsSheet.Cells[fieldsSheet.Dimension.Address].AutoFitColumns();

            return package.GetAsByteArray();
        }

        private static void AddListValidation(ExcelWorksheet sheet, int fromRow, int toRow, int col, string csvValues)
        {
            var range = sheet.Cells[fromRow, col, toRow, col];
            var validation = sheet.DataValidations.AddListValidation(range.Address);
            foreach (var val in csvValues.Split(','))
                validation.Formula.Values.Add(val);
            validation.ShowErrorMessage = true;
            validation.ErrorTitle = "Invalid value";
            validation.Error = $"Please select from: {csvValues}";
        }

        private static void AddSheetReferenceValidation(ExcelWorksheet sheet, int fromRow, int toRow, int col, string sourceSheet, string sourceRange)
        {
            var range = sheet.Cells[fromRow, col, toRow, col];
            var validation = sheet.DataValidations.AddListValidation(range.Address);
            validation.Formula.ExcelFormula = $"'{sourceSheet}'!{sourceRange}";
            validation.ShowErrorMessage = true;
            validation.AllowBlank = true;
        }

        private static void WriteHeaders(ExcelWorksheet sheet, params string[] headers)
        {
            for (var i = 0; i < headers.Length; i++)
            {
                var cell = sheet.Cells[1, i + 1];
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
            }
        }

        private static void WriteFieldSample(ExcelWorksheet sheet, int row,
            string formName, string sectionName, string fieldName, string fieldId,
            string fieldType, string isRequired, string validationRules, int order)
        {
            sheet.Cells[row, 1].Value = formName;
            sheet.Cells[row, 2].Value = sectionName;
            sheet.Cells[row, 3].Value = fieldName;
            sheet.Cells[row, 4].Value = fieldId;
            sheet.Cells[row, 5].Value = fieldType;
            sheet.Cells[row, 6].Value = isRequired;
            sheet.Cells[row, 7].Value = validationRules;
            sheet.Cells[row, 8].Value = order;
        }

        // ── Data Export ──

        public Task<byte[]> GenerateExcelAsync(string formName, List<DynamicFormFieldDefinitionDto> fieldDefinitions, List<Dictionary<Guid, string?>>? rows = null)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(formName);

            for (var i = 0; i < fieldDefinitions.Count; i++)
            {
                var cell = worksheet.Cells[1, i + 1];
                cell.Value = fieldDefinitions[i].FieldName;
                cell.Style.Font.Bold = true;
            }

            if (rows != null)
            {
                for (var rowIdx = 0; rowIdx < rows.Count; rowIdx++)
                {
                    var rowData = rows[rowIdx];
                    for (var colIdx = 0; colIdx < fieldDefinitions.Count; colIdx++)
                    {
                        if (rowData.TryGetValue(fieldDefinitions[colIdx].Id, out var value) && value != null)
                            worksheet.Cells[rowIdx + 2, colIdx + 1].Value = value;
                    }
                }
            }

            if (worksheet.Dimension != null)
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            return Task.FromResult(package.GetAsByteArray());
        }

        // ── Simple Sheet-per-Form Parsing ──

        private static BulkCreateDynamicFormDto ParseSheetPerFormFormat(ExcelPackage package)
        {
            var result = new BulkCreateDynamicFormDto();

            foreach (var worksheet in package.Workbook.Worksheets)
            {
                if (worksheet.Dimension == null) continue;

                var rowCount = worksheet.Dimension.Rows;
                var colCount = worksheet.Dimension.Columns;
                if (rowCount < 1 || colCount < 1) continue;

                var headers = new List<string>();
                for (var col = 1; col <= colCount; col++)
                {
                    var header = worksheet.Cells[1, col].Text?.Trim();
                    headers.Add(string.IsNullOrWhiteSpace(header) ? $"Column{col}" : header);
                }

                var fieldDefinitions = headers.Select((h, idx) => new CreateDynamicFormFieldDefinitionDto
                {
                    FieldName = h,
                    FieldId = $"field_{idx + 1}_{Guid.NewGuid():N}"[..30],
                    FieldType = InferFieldType(worksheet, idx + 1, rowCount),
                    IsRequired = false
                }).ToList();

                var configJson = JsonSerializer.Serialize(new
                {
                    fields = fieldDefinitions.Select(fd => new { name = fd.FieldName, type = fd.FieldType })
                });

                result.Forms.Add(new CreateDynamicFormDto
                {
                    Name = worksheet.Name,
                    Description = $"Imported from Excel sheet: {worksheet.Name}",
                    ConfigJson = configJson,
                    IsActive = true,
                    FieldDefinitions = fieldDefinitions
                });
            }

            return result;
        }

        // ── Helpers ──

        private static string InferFieldType(ExcelWorksheet worksheet, int col, int rowCount)
        {
            var sampleSize = Math.Min(rowCount - 1, 10);
            if (sampleSize <= 0) return "String";

            var hasDecimal = false;
            var allNumeric = true;
            var allDate = true;
            var allBool = true;

            for (var row = 2; row <= 1 + sampleSize; row++)
            {
                var text = worksheet.Cells[row, col].Text?.Trim();
                if (string.IsNullOrWhiteSpace(text)) continue;

                if (!decimal.TryParse(text, out _))
                    allNumeric = false;
                else if (text.Contains('.'))
                    hasDecimal = true;

                if (!DateTime.TryParse(text, out _))
                    allDate = false;

                if (!bool.TryParse(text, out _))
                    allBool = false;
            }

            if (allBool) return "Boolean";
            if (allDate) return "DateTime";
            if (allNumeric) return hasDecimal ? "Decimal" : "Int";
            return "String";
        }

        private class SectionRow
        {
            public string Name { get; set; } = "";
            public string? Description { get; set; }
            public int Order { get; set; }
        }

        private class FieldRow
        {
            public string FieldName { get; set; } = "";
            public string FieldId { get; set; } = "";
            public string FieldType { get; set; } = "String";
            public bool IsRequired { get; set; }
            public string? ValidationRules { get; set; }
            public int Order { get; set; }
        }
    }
}
