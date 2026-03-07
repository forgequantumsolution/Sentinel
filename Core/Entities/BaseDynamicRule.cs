using Analytics_BE.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Analytics_BE.Core.Entities
{
    public abstract class BaseDynamicRule<T> : TenantEntity where T : class
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Field { get; set; } = string.Empty; // e.g., "User.Role.Name", "User.Department"

        [Required]
        public RuleOperator Operator { get; set; }

        [Required]
        public string Value { get; set; } = string.Empty; // The value to compare against

        [Required]
        public bool IsDynamicValue { get; set; } = false;

        // Composite rule properties
        public RuleType RuleType { get; set; } = RuleType.Simple;

        public Guid? ParentRuleId { get; set; }

        [ForeignKey("ParentRuleId")]
        public virtual T? ParentRule { get; set; }

        public virtual ICollection<T> ChildRules { get; set; } = [];

        // Navigation properties
        public Guid? UserGroupId { get; set; }

        [ForeignKey("UserGroupId")]
        public virtual UserGroup? UserGroup { get; set; }

        protected bool EvaluateCondition(object? fieldValue, object? expectedValue, RuleOperator op)
        {
            // Normalize values
            string fieldStr = fieldValue?.ToString() ?? string.Empty;
            string expectedStr = expectedValue?.ToString() ?? string.Empty;

            switch (op)
            {
                case RuleOperator.Equals:
                    return CompareEquals(fieldValue, expectedValue);

                case RuleOperator.NotEquals:
                    return !CompareEquals(fieldValue, expectedValue);

                case RuleOperator.Contains:
                    return fieldStr.Contains(expectedStr, StringComparison.OrdinalIgnoreCase);

                case RuleOperator.StartsWith:
                    return fieldStr.StartsWith(expectedStr, StringComparison.OrdinalIgnoreCase);

                case RuleOperator.EndsWith:
                    return fieldStr.EndsWith(expectedStr, StringComparison.OrdinalIgnoreCase);

                case RuleOperator.In:
                    return expectedStr.Split(',').Any(v => v.Trim().Equals(fieldStr, StringComparison.OrdinalIgnoreCase));

                case RuleOperator.NotIn:
                    return !expectedStr.Split(',').Any(v => v.Trim().Equals(fieldStr, StringComparison.OrdinalIgnoreCase));

                case RuleOperator.GreaterThan:
                    return CompareGreater(fieldStr, expectedStr);

                case RuleOperator.LessThan:
                    return CompareLess(fieldStr, expectedStr);

                case RuleOperator.GreaterThanOrEqual:
                    return CompareGreaterOrEqual(fieldStr, expectedStr);

                case RuleOperator.LessThanOrEqual:
                    return CompareLessOrEqual(fieldStr, expectedStr);

                default:
                    return false;
            }
        }

        private bool CompareEquals(object? a, object? b)
        {
            if (DateTime.TryParse(a?.ToString(), out var da) && DateTime.TryParse(b?.ToString(), out var db))
                return da == db;

            if (double.TryParse(a?.ToString(), out var na) && double.TryParse(b?.ToString(), out var nb))
                return na == nb;

            return string.Equals(a?.ToString(), b?.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private bool CompareGreater(string a, string b)
        {
            if (DateTime.TryParse(a, out var da) && DateTime.TryParse(b, out var db))
                return da > db;

            if (double.TryParse(a, out var na) && double.TryParse(b, out var nb))
                return na > nb;

            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase) > 0;
        }

        private bool CompareLess(string a, string b)
        {
            if (DateTime.TryParse(a, out var da) && DateTime.TryParse(b, out var db))
                return da < db;

            if (double.TryParse(a, out var na) && double.TryParse(b, out var nb))
                return na < nb;

            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase) < 0;
        }

        private bool CompareGreaterOrEqual(string a, string b)
        {
            return CompareGreater(a, b) || CompareEquals(a, b);
        }

        private bool CompareLessOrEqual(string a, string b)
        {
            return CompareLess(a, b) || CompareEquals(a, b);
        }

        protected bool EvaluateCompositeRule(User user)
        {
            if (ChildRules == null || !ChildRules.Any())
                return false;

            var evaluatedResults = new List<bool>();

            foreach (var childRule in ChildRules)
            {
                var childBase = childRule as BaseDynamicRule<T>;
                if (childBase != null && childBase.IsActive)
                {
                    evaluatedResults.Add(childBase.Evaluate(user));
                }
            }

            return RuleType switch
            {
                RuleType.And => evaluatedResults.All(r => r),
                RuleType.Or => evaluatedResults.Any(r => r),
                _ => false
            };
        }

        public bool Evaluate(User user, object? valObj = null)
        {
            if (!IsActive) return false;

            if (RuleType != RuleType.Simple)
            {
                return EvaluateCompositeRule(user);
            }

            var fieldValue = ResolveValue(user, Field);
            if (fieldValue == null) return false;

            var expectedValue = IsDynamicValue ? ResolveValue(valObj, Value) : Value;
            return EvaluateCondition(fieldValue, expectedValue, Operator);
        }

        private object? ResolveValue(object? source, string path)
        {
            if (source == null || string.IsNullOrWhiteSpace(path))
                return null;

            var parts = path.Split('.');
            object? current = source;

            foreach (var part in parts)
            {
                if (current == null) return null;

                var type = current.GetType();
                if (type.Name == part) continue;

                var property = type.GetProperty(part);
                if (property == null) return null;

                current = property.GetValue(current);
            }

            return current;
        }
    }
}
