using System.Reflection;
using System.Text.RegularExpressions;
using Core.Entities;

namespace Application.Common
{
    /// <summary>
    /// Constants and utilities for DynamicForm operations.
    /// </summary>
    public static class DynamicFormConstants
    {
        /// <summary>
        /// Cached count of available Field columns in DynamicFormRecord entity.
        /// Computed once using reflection and cached for performance.
        /// </summary>
        private static readonly Lazy<int> _maxColumnCount = new Lazy<int>(() =>
        {
            // Use reflection to count properties matching the pattern "Field{N}" in DynamicFormRecord
            var fieldPattern = new Regex(@"^Field\d+$", RegexOptions.Compiled);
            return typeof(DynamicFormRecord)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Count(p => fieldPattern.IsMatch(p.Name));
        });

        /// <summary>
        /// Gets the maximum number of columns available for field definitions.
        /// This is dynamically computed from DynamicFormRecord entity using reflection.
        /// </summary>
        public static int MaxColumnCount => _maxColumnCount.Value;
    }
}
