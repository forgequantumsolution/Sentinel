using System.Collections.Generic;

namespace Core.Models
{
    public class GraphConfigRequest
    {
        public string TableName { get; set; } = string.Empty;
        
        // e.g., COUNT(*), SUM(Amount), AVG(Age)
        public string AggregateFunction { get; set; } = string.Empty; 

        // The column to aggregate, if applicable. (e.g., 'Amount' for SUM(Amount))
        public string? AggregateColumn { get; set; }

        // e.g., 'DepartmentId', 'Status' - what will be on the X-axis/categories
        public string GroupByColumn { get; set; } = string.Empty;

        // Any custom WHERE clauses or filters to apply (sanitize in real app)
        // For simplicity, sticking to basic filters.
        public List<GraphFilter> Filters { get; set; } = new List<GraphFilter>();

        public string ChartType { get; set; } = "bar"; // bar, line, pie, etc.
    }

    public class GraphFilter
    {
        public string Column { get; set; } = string.Empty;
        public string Operator { get; set; } = "="; // =, >, <, LIKE
        public string Value { get; set; } = string.Empty;
    }
}
