using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Core.Models;

namespace Core.Entities;

public class GraphDataDefinitionEntity : TenantEntity
{
    /// <summary>FK → GraphConfigs.Id (one-to-one)</summary>
    [Required]
    public Guid GraphConfigId { get; set; }

    // ─── JSON Columns (serialized as string, DB-agnostic) ────────────────────

    public DataSourceDefinition Source { get; set; } = new();

    public List<SeriesCalculation> SeriesCalculations { get; set; } = new();

    public FilterGroup? GlobalFilter { get; set; }

    public List<SortRule>? SortRules { get; set; }

    public int? RowLimit { get; set; }

    // ─── Navigation ───────────────────────────────────────────────────────────
    [ForeignKey("GraphConfigId")]
    public GraphConfigEntity? GraphConfig { get; set; }
}
