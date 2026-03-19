namespace Core.Models
{
    /// <summary>
    /// Configuration schema for a KPI card component.
    /// Stored as JSON inside UiComponentEntity.ConfigJson.
    /// The backend treats ConfigJson as an opaque string; this class
    /// documents the expected shape for KpiCard component type.
    /// </summary>
    public class KpiCardConfig
    {
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }

        /// <summary>Icon name or URL.</summary>
        public string? Icon { get; set; }

        /// <summary>CSS color, hex, or theme token.</summary>
        public string? ColorScheme { get; set; }

        public KpiValueConfig Value { get; set; } = new();

        public KpiTrendConfig? Trend { get; set; }

        /// <summary>Any extra key-value pairs for rendering hints.</summary>
        public Dictionary<string, object>? Meta { get; set; }
    }

    public class KpiValueConfig
    {
        /// <summary>Static value shown when no data source is wired.</summary>
        public string? StaticValue { get; set; }

        /// <summary>Format pattern, e.g. "$#,##0", "0.00%".</summary>
        public string? Format { get; set; }

        public string? Prefix { get; set; }
        public string? Suffix { get; set; }
    }

    public class KpiTrendConfig
    {
        /// <summary>Percentage change shown next to the value.</summary>
        public double? ChangePercent { get; set; }

        /// <summary>"up" | "down" | "neutral"</summary>
        public string? Direction { get; set; }

        public string? Label { get; set; }
    }
}
