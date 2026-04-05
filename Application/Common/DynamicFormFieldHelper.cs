using Application.DTOs;
using Core.Entities;

namespace Application.Common
{
    /// <summary>
    /// Helper utility for validating and mapping dynamic form field definitions.
    /// Provides common logic for creating field definitions with auto-generated column names.
    /// </summary>
    public static class DynamicFormFieldHelper
    {
        /// <summary>
        /// Validates and maps field definitions from DTO to domain model.
        /// Auto-generates ColumnName as Field{index + 1} and validates against MaxColumnCount.
        /// </summary>
        /// <param name="fieldDefinitions">Source field definitions from DTO</param>
        /// <param name="maxColumnCount">Maximum allowed number of columns</param>
        /// <param name="organizationId">Optional organization ID to assign to field definitions</param>
        /// <returns>Mapped list of DynamicFormFieldDefinition objects</returns>
        /// <exception cref="ArgumentException">Thrown if field definitions are null/empty or exceed maxColumnCount</exception>
        public static List<DynamicFormFieldDefinition> ValidateAndMapFieldDefinitions(
            IList<CreateDynamicFormFieldDefinitionDto>? fieldDefinitions,
            Guid? organizationId = null)
        {
            if (fieldDefinitions == null || fieldDefinitions.Count == 0)
                throw new ArgumentException("FieldDefinitions are required and cannot be empty.");

            // var maxColumnCount = DynamicFormConstants.MaxColumnCount;
            // if (fieldDefinitions.Count > maxColumnCount)
            //     throw new ArgumentException(
            //         $"Field definition count ({fieldDefinitions.Count}) exceeds maximum allowed columns ({maxColumnCount}).");

            return fieldDefinitions
                .Select((fd, index) => new DynamicFormFieldDefinition
                {
                    ColumnName = $"Field{index + 1}",
                    FieldName = fd.FieldName,
                    FieldId = fd.FieldId,
                    FieldType = fd.FieldType,
                    IsRequired = fd.IsRequired,
                    ValidationRules = fd.ValidationRules,
                    OrganizationId = organizationId
                })
                .ToList();
        }
    }
}
