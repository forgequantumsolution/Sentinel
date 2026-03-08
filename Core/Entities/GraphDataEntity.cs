using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Core.Models;

namespace Core.Entities;

public class GraphDataDefinitionEntity : TenantEntity
{

    /// <summary>FK → GraphConfigs.Id (one-to-one)</summary>
    [Required]
    public string GraphConfigId { get; set; } = string.Empty;

    // ─── JSON Columns ─────────────────────────────────────────────────────────

    [Column("Source", TypeName = "jsonb")]               // use nvarchar(max) for SQL Server
    public DataSourceDefinition Source { get; set; } = new();

    [Column("SeriesCalculations", TypeName = "jsonb")]
    public List<SeriesCalculation> SeriesCalculations { get; set; } = new();

    [Column("GlobalFilter", TypeName = "jsonb")]
    public FilterGroup? GlobalFilter { get; set; }

    [Column("SortRules", TypeName = "jsonb")]
    public List<SortRule>? SortRules { get; set; }

    public int? RowLimit { get; set; }

    // ─── Navigation ───────────────────────────────────────────────────────────
    [ForeignKey("GraphConfigId")]
    public GraphConfigEntity? GraphConfig { get; set; }
}