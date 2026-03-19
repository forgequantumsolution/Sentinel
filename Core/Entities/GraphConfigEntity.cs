using System.ComponentModel.DataAnnotations.Schema;
using Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class GraphConfigEntity : TenantEntity
{
    [Required]
    public string Name { get; set; } = string.Empty;

    // ─── Discriminator ───────────────────────────────────────────────────────

    /// <summary>
    /// Null → this record is a graph (GraphType.Type is relevant).
    /// Set → this is a UI component (KpiCard, Table, etc.) and Type is ignored.
    /// </summary>
    public UiComponentType? ComponentType { get; set; }

    // ─── Graph-specific ──────────────────────────────────────────────────────

    /// <summary>Only meaningful when ComponentType is null.</summary>
    public GraphType Type { get; set; }

    // ─── Generic JSON storage ────────────────────────────────────────────────
    // Both columns are opaque jsonb — the backend stores and returns them as-is.
    // For graph configs the expected shape is documented in Core.Models.GraphViewConfig
    // and Core.Models.GraphDataConfig.
    // For KPI cards the expected shape is documented in Core.Models.KpiCardConfig
    // (View = card visual config, Data = data-source config).

    /// <summary>Visual / presentation config stored as raw JSON string.</summary>
    public string View { get; set; } = "{}";

    /// <summary>Data source / calculation config stored as raw JSON string.</summary>
    public string Data { get; set; } = "{}";

    public Dictionary<string, object>? Meta { get; set; }

    // ─── Folder placement via ActionObject ──────────────────────────────────

    public Guid? ActionObjectId { get; set; }

    [ForeignKey("ActionObjectId")]
    public virtual ActionObject? ActionObject { get; set; }
}
