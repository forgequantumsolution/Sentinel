using Core.Enums;

namespace Core.Models
{
    /// <summary>
    /// Main payload for graph configuration and data
    /// </summary>
    public class GraphPayload
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public GraphType Type { get; set; }
        public GraphViewConfig View { get; set; } = new();
        public GraphDataConfig Data { get; set; } = new();
        public Dictionary<string, object>? Meta { get; set; }  // any extra key-value pairs
    }

    /// <summary>
    /// Configuration for graph visual presentation
    /// </summary>
    public class GraphViewConfig
    {
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? ColorScheme { get; set; }  // e.g. "default", "pastel", "#hex,#hex"
        public DimensionConfig Dimensions { get; set; } = new();
        public LegendConfig Legend { get; set; } = new();
        public AxisConfig? XAxis { get; set; }  // null for Pie/Donut
        public AxisConfig? YAxis { get; set; }
        public TooltipConfig Tooltip { get; set; } = new();
        public bool Animated { get; set; } = true;
        public bool Responsive { get; set; } = true;
    }

    /// <summary>
    /// Chart dimensions configuration
    /// </summary>
    public class DimensionConfig
    {
        public int? Width { get; set; }   // null = 100%
        public int? Height { get; set; }
    }

    /// <summary>
    /// Legend display configuration
    /// </summary>
    public class LegendConfig
    {
        public bool Visible { get; set; } = true;
        public LegendPosition Position { get; set; } = LegendPosition.Bottom;
    }

    /// <summary>
    /// Axis configuration for X and Y axes
    /// </summary>
    public class AxisConfig
    {
        public string? Label { get; set; }
        public AxisScaleType ScaleType { get; set; } = AxisScaleType.Linear;
        public object? Min { get; set; }
        public object? Max { get; set; }
        public string? Format { get; set; }  // e.g. "MM/yyyy", "0.00", "$#,##0"
        public bool GridLines { get; set; } = true;
    }

    /// <summary>
    /// Tooltip display configuration
    /// </summary>
    public class TooltipConfig
    {
        public bool Enabled { get; set; } = true;
        public string? Format { get; set; }  // template string e.g. "{label}: {value}"
    }
}
