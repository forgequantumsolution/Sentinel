using Core.Enums;

namespace Core.Models;

/// <summary>
/// Configuration for graph data
/// </summary>
public class GraphDataConfig
{
    public List<string> Labels { get; set; } = new();  // X-axis / slice labels
    public List<GraphSeries> Series { get; set; } = new();
}

/// <summary>
/// Represents a data series within a graph
/// </summary>
public class GraphSeries
{
    public string Name { get; set; } = string.Empty;
    public string? Color { get; set; }  // override per series
    public List<DataPoint> Points { get; set; } = new();
}

/// <summary>
/// Represents a single data point in a series
/// </summary>
public class DataPoint
{
    public object? X { get; set; }  // can be number, string, or DateTime
    public object Y { get; set; } = 0;
    public object? Z { get; set; }  // for Bubble charts (size)
    public string? Label { get; set; }  // optional override label for this point
}

// ─── Data Source ──────────────────────────────────────────────────────────────
public class DataSourceDefinition
{
    public DataSourceType Type { get; set; }

    // SQL / StoredProcedure
    public string? ConnectionStringName { get; set; }  // key from config
    public string? SqlQuery { get; set; }  // raw SQL or SP name
    public Dictionary<string, object>? SqlParameters { get; set; }

    // REST API
    public string? ApiUrl { get; set; }
    public string? HttpMethod { get; set; } = "GET";
    public Dictionary<string, string>? Headers { get; set; }
    public Dictionary<string, string>? QueryParams { get; set; }
    public string? ResponseDataPath { get; set; }  // e.g. "$.data.items"

    // CSV / File
    public string? FilePath { get; set; }
    public bool HasHeader { get; set; } = true;
    public char Delimiter { get; set; } = ',';

    // DynamicForm — pull data from EAV form submissions
    public DynamicFormSourceConfig? DynamicForm { get; set; }
}

// ─── Dynamic Form Source Config ───────────────────────────────────────────────

/// <summary>
/// Configuration for sourcing data from EAV dynamic form submissions via FormQueryEngine.
/// Write a FormQuery SQL statement (SELECT ... FROM FormName WHERE ...) and it will be
/// executed by FormQueryEngine — field names in SeriesCalculation map to the result columns.
/// </summary>
public class DynamicFormSourceConfig
{
    /// <summary>
    /// FormQuery SQL statement passed directly to FormQueryEngine.
    /// e.g. "SELECT month, SUM(revenue) AS total FROM SalesForm GROUP BY month"
    /// </summary>
    public string FormQuerySql { get; set; } = string.Empty;

    /// <summary>
    /// Optional named parameters referenced in the FormQuery SQL (e.g. @startDate).
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }
}

// ─── Series Calculation ───────────────────────────────────────────────────────

/// <summary>
/// Defines how one series gets its X/Y/Z values from the raw source data.
/// Maps 1-to-1 with GraphSeries by SeriesName.
/// </summary>
public class SeriesCalculation
{
    /// <summary>Must match GraphSeries.Name exactly.</summary>
    public string SeriesName { get; set; } = string.Empty;

    /// <summary>Column/field to use for X axis (or pie slice label).</summary>
    public FieldMapping XField { get; set; } = new();

    /// <summary>Column/field to use for Y axis (the value).</summary>
    public FieldMapping YField { get; set; } = new();

    /// <summary>Optional Z field (Bubble chart size).</summary>
    public FieldMapping? ZField { get; set; }

    /// <summary>Group rows by these fields before aggregating.</summary>
    public List<string>? GroupByFields { get; set; }

    /// <summary>Filter applied only to this series (on top of GlobalFilter).</summary>
    public FilterGroup? SeriesFilter { get; set; }

    /// <summary>Optional formula applied to Y after aggregation.</summary>
    public string? PostFormula { get; set; }   // e.g.  "value * 100 / total"
}

// ─── Field Mapping ────────────────────────────────────────────────────────────

public class FieldMapping
{
    /// <summary>Column name / JSON path from the source.</summary>
    public string FieldName { get; set; } = string.Empty;

    public AggregationType Aggregation { get; set; } = AggregationType.None;

    /// <summary>Optional display format applied to this field's values.</summary>
    public string? Format { get; set; }   // "MM/yyyy", "0.00", "$#,##0"

    /// <summary>Static value override — skips FieldName when set.</summary>
    public object? StaticValue { get; set; }
}


// ─── Filtering ────────────────────────────────────────────────────────────────

/// <summary>A group of filter conditions joined by AND / OR.</summary>
public class FilterGroup
{
    public JoinOperator Join { get; set; } = JoinOperator.And;
    public List<FilterRule> Rules { get; set; } = new();

    /// <summary>Nested groups for complex expressions e.g. (A AND B) OR (C AND D).</summary>
    public List<FilterGroup>? SubGroups { get; set; }
}

public class FilterRule
{
    public string Field { get; set; } = string.Empty;
    public FilterOperator Operator { get; set; }

    /// <summary>Single value for Eq/Gt/Like etc.</summary>
    public object? Value { get; set; }

    /// <summary>Multiple values for In / NotIn.</summary>
    public List<object>? Values { get; set; }

    /// <summary>
    /// Reference another field instead of a static value.
    /// e.g. filter where "actual_date" == "@today"
    /// </summary>
    public string? ValueRef { get; set; }  // "@today", "@startOfMonth"
}

// ─── Sorting ──────────────────────────────────────────────────────────────────

public class SortRule
{
    public string Field { get; set; } = string.Empty;
    public SortDirection Direction { get; set; } = SortDirection.Asc;
}