using System;

namespace Analytics_BE.Application.DTOs
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

    public class CreateDynamicFormDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ConfigJson { get; set; }
        public bool IsActive { get; set; } = true;
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
