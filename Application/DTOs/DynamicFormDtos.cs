using System;

namespace Application.DTOs
{
    public class DynamicFormDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ConfigJson { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateDynamicFormFieldDefinitionDto
    {
        public string ColumnName { get; set; }
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public bool IsRequired { get; set; }
        public string? ValidationRules { get; set; }
    }

    public class CreateDynamicFormDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ConfigJson { get; set; }
        public bool IsActive { get; set; } = true;
        public System.Collections.Generic.List<CreateDynamicFormFieldDefinitionDto>? FieldDefinitions { get; set; }
    }

    public class DynamicFormSubmissionDto
    {
        public Guid Id { get; set; }
        public Guid FormId { get; set; }
        public string DataJson { get; set; }
        public Guid? CreatedById { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateDynamicFormSubmissionDto
    {
        public string DataJson { get; set; }
    }
}
