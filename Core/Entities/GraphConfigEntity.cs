using Core.Enums;
using Core.Models;
using System.ComponentModel.DataAnnotations;

namespace Core.Entities;

public class GraphConfigEntity : TenantEntity
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public GraphType Type { get; set; }

    // ─── JSON stored as text (DB-agnostic) ──────────────────────────────────
    // Serialization handled via EF Core value conversions in AppDbContext.

    public GraphViewConfig View { get; set; } = new();

    public GraphDataConfig Data { get; set; } = new();

    public Dictionary<string, object>? Meta { get; set; }
}
