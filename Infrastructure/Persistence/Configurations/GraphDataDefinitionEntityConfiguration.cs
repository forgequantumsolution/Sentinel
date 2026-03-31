using System.Text.Json;
using Core.Entities;
using Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class GraphDataDefinitionEntityConfiguration : IEntityTypeConfiguration<GraphDataDefinitionEntity>
{
    public void Configure(EntityTypeBuilder<GraphDataDefinitionEntity> builder)
    {
        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        builder.Property(e => e.Source)
            .HasColumnName("Source")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<DataSourceDefinition>(v, jsonOptions) ?? new(),
                new ValueComparer<DataSourceDefinition>(
                    (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
                    v => JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                    v => JsonSerializer.Deserialize<DataSourceDefinition>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions) ?? new()));

        builder.Property(e => e.SeriesCalculations)
            .HasColumnName("SeriesCalculations")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, jsonOptions),
                v => JsonSerializer.Deserialize<List<SeriesCalculation>>(v, jsonOptions) ?? new(),
                new ValueComparer<List<SeriesCalculation>>(
                    (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
                    v => JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                    v => JsonSerializer.Deserialize<List<SeriesCalculation>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions) ?? new()));

        builder.Property(e => e.GlobalFilter)
            .HasColumnName("GlobalFilter")
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, jsonOptions),
                v => v == null ? null : JsonSerializer.Deserialize<FilterGroup>(v, jsonOptions),
                new ValueComparer<FilterGroup?>(
                    (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
                    v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                    v => v == null ? null : JsonSerializer.Deserialize<FilterGroup>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions)));

        builder.Property(e => e.SortRules)
            .HasColumnName("SortRules")
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, jsonOptions),
                v => v == null ? null : JsonSerializer.Deserialize<List<SortRule>>(v, jsonOptions),
                new ValueComparer<List<SortRule>?>(
                    (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
                    v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
                    v => v == null ? null : JsonSerializer.Deserialize<List<SortRule>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions)));
    }
}
