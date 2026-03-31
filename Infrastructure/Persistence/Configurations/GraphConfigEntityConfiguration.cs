using System.Text.Json;
using Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class GraphConfigEntityConfiguration : IEntityTypeConfiguration<GraphConfigEntity>
{
    public void Configure(EntityTypeBuilder<GraphConfigEntity> builder)
    {
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        builder.Property(e => e.View)
            .HasColumnName("View")
            .HasColumnType("jsonb")
            .HasConversion(
                v => v,
                v => v ?? "{}",
                new ValueComparer<string>(
                    (a, b) => a == b,
                    v => v == null ? 0 : v.GetHashCode(),
                    v => v));

        builder.Property(e => e.Data)
            .HasColumnName("Data")
            .HasColumnType("jsonb")
            .HasConversion(
                v => v,
                v => v ?? "{}",
                new ValueComparer<string>(
                    (a, b) => a == b,
                    v => v == null ? 0 : v.GetHashCode(),
                    v => v));

        builder.Property(e => e.Meta)
            .HasColumnName("Meta")
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, jsonOptions),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, jsonOptions),
                new ValueComparer<Dictionary<string, object>?>(
                    (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
                    v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                    v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions)));

        builder.Property(e => e.FiltersParams)
            .HasColumnName("FiltersParams")
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, jsonOptions),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, jsonOptions),
                new ValueComparer<Dictionary<string, object>?>(
                    (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
                    v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                    v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions)));
    }
}
