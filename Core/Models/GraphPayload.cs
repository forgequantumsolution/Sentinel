using Core.Enums;
using System.Text.Json;

namespace Core.Models
{
    /// <summary>
    /// Payload returned by the graph/component payload and execute endpoints.
    /// View and Data are passed through as raw JSON — the backend does not parse them.
    /// </summary>
    public class GraphPayload
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public GraphType Type { get; set; }
        public int? ComponentType { get; set; }

        /// <summary>Visual config — raw JSON, shape depends on ComponentType.</summary>
        public JsonElement? View { get; set; }

        /// <summary>Data source config — raw JSON, shape depends on ComponentType.</summary>
        public JsonElement? Data { get; set; }

        public Dictionary<string, object>? Meta { get; set; }
    }

    // ── Reference schemas ────────────────────────────────────────────────────────
    // The classes below document the expected JSON shape for each component type.
    // They are NOT used by entity or DTO code — they exist only as a contract
    // reference for frontend consumers.
    //
    // Graph visual config  → GraphViewConfig  (see below)
    // Graph data config    → GraphDataConfig  (Core/Models/GraphDataConfig.cs)
    // KPI card config      → KpiCardConfig    (Core/Models/KpiCardConfig.cs)

    /// <summary>Reference schema for graph visual config (ComponentType == null).</summary>
    public class GraphViewConfig
    {
        public string? Title { get; set; }
        public string? Subtitle { get; set; }
        public string? ColorScheme { get; set; }
        public DimensionConfig Dimensions { get; set; } = new();
        public LegendConfig Legend { get; set; } = new();
        public AxisConfig? XAxis { get; set; }
        public AxisConfig? YAxis { get; set; }
        public TooltipConfig Tooltip { get; set; } = new();
        public bool Animated { get; set; } = true;
        public bool Responsive { get; set; } = true;
    }

    public class DimensionConfig
    {
        public int? Width { get; set; }
        public int? Height { get; set; }
    }

    public class LegendConfig
    {
        public bool Visible { get; set; } = true;
        public LegendPosition Position { get; set; } = LegendPosition.Bottom;
    }

    public class AxisConfig
    {
        public string? Label { get; set; }
        public AxisScaleType ScaleType { get; set; } = AxisScaleType.Linear;
        public object? Min { get; set; }
        public object? Max { get; set; }
        public string? Format { get; set; }
        public bool GridLines { get; set; } = true;
    }

    public class TooltipConfig
    {
        public bool Enabled { get; set; } = true;
        public string? Format { get; set; }
    }
}
