namespace Core.Enums;

/// <summary>
/// Supported chart/graph types
/// </summary>
public enum GraphType
{
    Line,
    Bar,
    StackedBar,
    Pie,
    Donut,
    Area,
    Scatter,
    Bubble,
    Radar,
    Heatmap
}

/// <summary>
/// Legend position options for charts
/// </summary>
public enum LegendPosition
{
    Top,
    Bottom,
    Left,
    Right,
    None
}

/// <summary>
/// Axis scale type for chart axes
/// </summary>
public enum AxisScaleType
{
    Linear,
    Logarithmic,
    DateTime,
    Category
}

public enum DataSourceType { SqlQuery, RestApi, InMemory, CsvFile, StoredProcedure, DynamicForm }
public enum AggregationType { None, Sum, Avg, Count, Min, Max, CountDistinct }
public enum SortDirection { Asc, Desc }
public enum FilterOperator { Eq, NotEq, Gt, Gte, Lt, Lte, In, NotIn, Like, IsNull, IsNotNull }
public enum JoinOperator { And, Or }
