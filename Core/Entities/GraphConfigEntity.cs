using Core.Enums;
using Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities;

public class GraphConfigEntity : TenantEntity
{
    // ─── Identity ─────────────────────────────────────────────────────────────

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public GraphType Type { get; set; }

    // ─── JSON Columns (EF Core 8 native JSON) ─────────────────────────────────

    [Column("View", TypeName = "jsonb")]   // use "nvarchar(max)" for SQL Server
    public GraphViewConfig View { get; set; } = new();

    [Column("Data", TypeName = "jsonb")]
    public GraphDataConfig Data { get; set; } = new();

    [Column("Meta", TypeName = "jsonb")]
    public Dictionary<string, object>? Meta { get; set; }

}