using Application.DTOs;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class RuleFieldService : IRuleFieldService
    {
        private static readonly List<RuleFieldOperatorDto> StringOperators =
        [
            new() { Value = 0,  Name = "Equals" },
            new() { Value = 1,  Name = "NotEquals" },
            new() { Value = 2,  Name = "Contains" },
            new() { Value = 3,  Name = "StartsWith" },
            new() { Value = 4,  Name = "EndsWith" },
            new() { Value = 5,  Name = "In" },
            new() { Value = 6,  Name = "NotIn" }
        ];

        private static readonly List<RuleFieldOperatorDto> NumericOperators =
        [
            new() { Value = 0,  Name = "Equals" },
            new() { Value = 1,  Name = "NotEquals" },
            new() { Value = 7,  Name = "GreaterThan" },
            new() { Value = 8,  Name = "LessThan" },
            new() { Value = 9,  Name = "GreaterThanOrEqual" },
            new() { Value = 10, Name = "LessThanOrEqual" }
        ];

        private static readonly List<RuleFieldOperatorDto> DateOperators =
        [
            new() { Value = 0,  Name = "Equals" },
            new() { Value = 1,  Name = "NotEquals" },
            new() { Value = 7,  Name = "GreaterThan" },
            new() { Value = 8,  Name = "LessThan" },
            new() { Value = 9,  Name = "GreaterThanOrEqual" },
            new() { Value = 10, Name = "LessThanOrEqual" }
        ];

        private static readonly List<RuleFieldDto> Fields =
        [
            new() { Field = "User.Role.Name",        Label = "Role",            Type = "string",  Operators = StringOperators },
            new() { Field = "User.Department.Name",  Label = "Department",      Type = "string",  Operators = StringOperators },
            new() { Field = "User.Department.Code",  Label = "Department Code", Type = "string",  Operators = StringOperators },
            new() { Field = "User.JobTitle.Title",   Label = "Job Title",       Type = "string",  Operators = StringOperators },
            new() { Field = "User.Name",             Label = "Name",            Type = "string",  Operators = StringOperators },
            new() { Field = "User.Email",            Label = "Email",           Type = "string",  Operators = StringOperators },
            new() { Field = "User.Location",         Label = "Location",        Type = "string",  Operators = StringOperators },
            new() { Field = "User.EmploymentType",   Label = "Employment Type", Type = "string",  Operators = StringOperators },
            new() { Field = "User.Division",         Label = "Division",        Type = "string",  Operators = StringOperators },
            new() { Field = "User.BusinessUnit",     Label = "Business Unit",   Type = "string",  Operators = StringOperators },
            new() { Field = "User.CostCenter",       Label = "Cost Center",     Type = "string",  Operators = StringOperators },
            new() { Field = "User.EmployeeId",       Label = "Employee ID",     Type = "string",  Operators = StringOperators },
            //new() { Field = "User.HireDate",         Label = "Hire Date",       Type = "date",    Operators = DateOperators },
            new() { Field = "User.Status",           Label = "Status",          Type = "string",  Operators = StringOperators }
        ];

        public List<RuleFieldDto> GetRuleFields() => Fields;
    }
}
