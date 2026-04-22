using System.Text.Json;
using Core.Entities;
using Core.Enums;
using Core.Models;
using Application.DTOs;
using Application.Common.Pagination;
using Application.FormQuery;
using Application.Interfaces;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;

namespace Application.Services
{
    public class GraphService : IGraphService
    {
        private readonly IGraphConfigRepository _graphConfigRepository;
        private readonly IGraphDataDefinitionRepository _graphDataDefinitionRepository;
        private readonly IActionObjectRepository _actionObjectRepository;
        private readonly IFormQueryEngine _formQueryEngine;
        private readonly ICsvDataSourceService _csvService;
        private readonly ITenantContext _tenantContext;

        public GraphService(
            IGraphConfigRepository graphConfigRepository,
            IGraphDataDefinitionRepository graphDataDefinitionRepository,
            IActionObjectRepository actionObjectRepository,
            IFormQueryEngine formQueryEngine,
            ICsvDataSourceService csvService,
            ITenantContext tenantContext)
        {
            _graphConfigRepository = graphConfigRepository;
            _graphDataDefinitionRepository = graphDataDefinitionRepository;
            _actionObjectRepository = actionObjectRepository;
            _formQueryEngine = formQueryEngine;
            _csvService = csvService;
            _tenantContext = tenantContext;
        }

        // GraphConfig operations
        public async Task<GraphConfigEntity?> GetGraphConfigByIdAsync(Guid id)
        {
            return await _graphConfigRepository.GetByIdAsync(id);
        }

        public async Task<GraphConfigEntity?> GetGraphConfigByNameAsync(string name)
        {
            return await _graphConfigRepository.GetByNameAsync(name);
        }

        public async Task<PagedResult<GraphConfigEntity>> GetAllGraphConfigsAsync(PageRequest pageRequest)
        {
            return await _graphConfigRepository.GetAllAsync(pageRequest);
        }

        public async Task<PagedResult<GraphConfigEntity>> GetGraphConfigsByTypeAsync(Core.Enums.GraphType type, PageRequest pageRequest)
        {
            return await _graphConfigRepository.GetByTypeAsync(type, pageRequest);
        }

        public async Task<GraphConfigEntity> CreateGraphConfigAsync(CreateGraphConfigRequest request)
        {
            var isUiComponent = request.ComponentType.HasValue;

            ActionObject? actionObject = null;
            if (request.ParentFolderId.HasValue)
            {
                var folder = await _actionObjectRepository.GetByIdAsync(request.ParentFolderId.Value);
                if (folder == null || folder.ObjectType != ObjectType.Folder)
                    throw new ArgumentException("Folder not found.");

                actionObject = new ActionObject
                {
                    Name = request.Name,
                    ObjectType = isUiComponent ? ObjectType.UIComponent : ObjectType.Graph,
                    ParentObjectId = request.ParentFolderId.Value,
                    CreatedAt = DateTime.UtcNow
                };
                await _actionObjectRepository.AddAsync(actionObject);
            }

            var graphConfig = new GraphConfigEntity
            {
                Name = request.Name,
                ComponentType = request.ComponentType.HasValue
                    ? (UiComponentType)request.ComponentType.Value
                    : null,
                Type = (GraphType)request.Type,
                View = request.View.HasValue ? request.View.Value.GetRawText() : "{}",
                Data = request.Data.HasValue ? request.Data.Value.GetRawText() : "{}",
                Meta = request.Meta,
                FiltersParams = request.FiltersParams,
                IsActive = request.IsActive,
                ActionObjectId = actionObject?.Id,
                CreatedAt = DateTime.UtcNow
            };

            await _graphConfigRepository.AddAsync(graphConfig);
            return graphConfig;
        }

        public async Task UpdateGraphConfigAsync(Guid id, UpdateGraphConfigRequest request)
        {
            var graphConfig = await _graphConfigRepository.GetByIdAsync(id);
            if (graphConfig == null)
                throw new KeyNotFoundException($"GraphConfig with id {id} not found");

            var isUiComponent = request.ComponentType.HasValue;

            graphConfig.Name = request.Name;
            graphConfig.ComponentType = request.ComponentType.HasValue
                ? (UiComponentType)request.ComponentType.Value
                : null;
            graphConfig.Type = (GraphType)request.Type;
            graphConfig.View = request.View.HasValue ? request.View.Value.GetRawText() : "{}";
            graphConfig.Data = request.Data.HasValue ? request.Data.Value.GetRawText() : "{}";
            graphConfig.Meta = request.Meta;
            graphConfig.FiltersParams = request.FiltersParams;
            graphConfig.IsActive = request.IsActive;
            graphConfig.UpdatedAt = DateTime.UtcNow;

            // Handle folder move
            if (request.ParentFolderId.HasValue)
            {
                if (graphConfig.ActionObjectId.HasValue)
                {
                    // Move existing ActionObject to new folder
                    var ao = await _actionObjectRepository.GetByIdAsync(graphConfig.ActionObjectId.Value);
                    if (ao != null)
                    {
                        ao.Name = request.Name;
                        ao.ParentObjectId = request.ParentFolderId.Value;
                        ao.UpdatedAt = DateTime.UtcNow;
                        await _actionObjectRepository.UpdateAsync(ao);
                    }
                }
                else
                {
                    // Create new ActionObject in folder
                    var ao = new ActionObject
                    {
                        Name = request.Name,
                        ObjectType = isUiComponent ? ObjectType.UIComponent : ObjectType.Graph,
                        ParentObjectId = request.ParentFolderId.Value,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _actionObjectRepository.AddAsync(ao);
                    graphConfig.ActionObjectId = ao.Id;
                }
            }

            await _graphConfigRepository.UpdateAsync(graphConfig);
        }

        public async Task<PagedResult<GraphConfigEntity>> GetUiComponentsByTypeAsync(UiComponentType componentType, PageRequest pageRequest)
        {
            return await _graphConfigRepository.GetByComponentTypeAsync(componentType, pageRequest);
        }

        public async Task DeleteGraphConfigAsync(Guid id)
        {
            // First delete associated data definitions
            await _graphDataDefinitionRepository.DeleteByGraphConfigIdAsync(id);
            // Then delete the config
            await _graphConfigRepository.DeleteAsync(id);
        }

        // GraphDataDefinition operations
        public async Task<GraphDataDefinitionEntity?> GetGraphDataDefinitionByIdAsync(Guid id)
        {
            return await _graphDataDefinitionRepository.GetByIdAsync(id);
        }

        public async Task<GraphDataDefinitionEntity?> GetGraphDataDefinitionByGraphConfigIdAsync(Guid graphConfigId)
        {
            return await _graphDataDefinitionRepository.GetByGraphConfigIdAsync(graphConfigId);
        }

        public async Task<PagedResult<GraphDataDefinitionEntity>> GetAllGraphDataDefinitionsAsync(PageRequest pageRequest)
        {
            return await _graphDataDefinitionRepository.GetAllAsync(pageRequest);
        }

        public async Task<GraphDataDefinitionEntity> CreateGraphDataDefinitionAsync(CreateGraphDataDefinitionRequest request)
        {
            var graphDataDefinition = new GraphDataDefinitionEntity
            {
                GraphConfigId = request.GraphConfigId,
                Source = request.Source,
                // TODO: will add once functionality is developed
                // SeriesCalculations = request.SeriesCalculations,
                // GlobalFilter = request.GlobalFilter,
                // SortRules = request.SortRules,
                // RowLimit = request.RowLimit,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _graphDataDefinitionRepository.AddAsync(graphDataDefinition);
            return graphDataDefinition;
        }

        public async Task UpdateGraphDataDefinitionAsync(Guid id, UpdateGraphDataDefinitionRequest request)
        {
            var graphDataDefinition = await _graphDataDefinitionRepository.GetByIdAsync(id);
            if (graphDataDefinition == null)
                throw new KeyNotFoundException($"GraphDataDefinition with id {id} not found");

            graphDataDefinition.Source = request.Source;
            // TODO: will add once functionality is developed
            // graphDataDefinition.SeriesCalculations = request.SeriesCalculations;
            // graphDataDefinition.GlobalFilter = request.GlobalFilter;
            // graphDataDefinition.SortRules = request.SortRules;
            // graphDataDefinition.RowLimit = request.RowLimit;
            graphDataDefinition.IsActive = request.IsActive;
            graphDataDefinition.UpdatedAt = DateTime.UtcNow;

            await _graphDataDefinitionRepository.UpdateAsync(graphDataDefinition);
        }

        public async Task DeleteGraphDataDefinitionAsync(Guid id)
        {
            await _graphDataDefinitionRepository.DeleteAsync(id);
        }

        public async Task DeleteGraphDataDefinitionsByGraphConfigIdAsync(Guid graphConfigId)
        {
            await _graphDataDefinitionRepository.DeleteByGraphConfigIdAsync(graphConfigId);
        }

        // Combined operations
        public async Task<GraphPayload> GetGraphPayloadAsync(Guid graphConfigId)
        {
            // Delegate to ExecuteGraphAsync with no runtime overrides —
            // if a data source exists it will be executed, otherwise falls back to static Data JSON.
            return await ExecuteGraphAsync(graphConfigId, null);
        }

        public async Task<GraphPayload> ExecuteGraphAsync(Guid graphConfigId, GraphExecuteRequest? request = null)
        {
            var graphConfig = await _graphConfigRepository.GetByIdAsync(graphConfigId);
            if (graphConfig == null)
                throw new KeyNotFoundException($"GraphConfig with id {graphConfigId} not found");

            var dataDef = await _graphDataDefinitionRepository.GetByGraphConfigIdAsync(graphConfigId);

            var payload = new GraphPayload
            {
                Id = graphConfig.Id.ToString(),
                Type = graphConfig.Type,
                ComponentType = graphConfig.ComponentType.HasValue ? (int)graphConfig.ComponentType.Value : null,
                View = ParseJsonElement(graphConfig.View),
                Meta = graphConfig.Meta,
                FiltersParams = graphConfig.FiltersParams
            };

            if (dataDef == null)
            {
                payload.Data = ParseJsonElement(graphConfig.Data);
                return payload;
            }

            if (dataDef.Source.Type == DataSourceType.LocalExcel)
            {
                payload.ClientSideCalc = true;
                payload.Data = dataDef.Source.LocalExcel?.Config ?? ParseJsonElement(graphConfig.Data);
                return payload;
            }

            // Merge FE filters with saved GlobalFilter
            var effectiveFilter = MergeFilters(dataDef.GlobalFilter, request?.Filters);
            var effectiveSortRules = request?.SortRules ?? dataDef.SortRules;
            var effectiveRowLimit = request?.RowLimit ?? dataDef.RowLimit;

            // Execute based on data source type
            FormQueryResult? queryResult = null;

            if (dataDef.Source.Type == DataSourceType.DynamicForm && dataDef.Source.DynamicForm != null)
            {
                // Merge parameters: DynamicForm defaults → runtime request (wins)
                var effectiveParams = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                if (dataDef.Source.DynamicForm.Parameters != null)
                    foreach (var kv in dataDef.Source.DynamicForm.Parameters)
                        effectiveParams[kv.Key] = kv.Value;
                if (request?.Parameters != null)
                    foreach (var kv in request.Parameters)
                        effectiveParams[kv.Key] = kv.Value;

                // Substitute @parameters into the SQL as string literals
                var baseSql = dataDef.Source.DynamicForm.FormQuerySql;
                baseSql = SubstituteParameters(baseSql, effectiveParams);

                var sql = BuildDynamicFormSql(baseSql, effectiveFilter, effectiveSortRules, effectiveRowLimit);

                var formRequest = new FormQueryRequest { Sql = sql };
                queryResult = await _formQueryEngine.ExecuteAsync(formRequest, _tenantContext.OrganizationId);
            }
            else if (dataDef.Source.Type == DataSourceType.SqlQuery && dataDef.Source.Sql != null && !string.IsNullOrWhiteSpace(dataDef.Source.Sql.Query))
            {
                var sql = dataDef.Source.Sql.Query;
                var formRequest = new FormQueryRequest { Sql = sql };
                queryResult = await _formQueryEngine.ExecuteAsync(formRequest, _tenantContext.OrganizationId);
            }
            else if (dataDef.Source.Type == DataSourceType.CsvFile && dataDef.Source.Csv != null)
            {
                queryResult = await _csvService.ExecuteAsync(dataDef.Source.Csv);
            }

            if (queryResult != null)
            {
                // Resolve series from active source type, fall back to dataDef level
                var seriesCalcs = dataDef.Source.Type switch
                {
                    DataSourceType.DynamicForm => dataDef.Source.DynamicForm?.Series,
                    DataSourceType.SqlQuery => dataDef.Source.Sql?.Series,
                    DataSourceType.RestApi => dataDef.Source.RestApi?.Series,
                    DataSourceType.CsvFile => dataDef.Source.Csv?.Series,
                    _ => null
                } ?? dataDef.SeriesCalculations;

                var graphData = TransformToGraphData(queryResult, seriesCalcs, graphConfig.Data);
                AssignSeriesColors(graphData, graphConfig.Data);
                payload.Data = JsonSerializer.SerializeToElement(graphData);
            }
            else
            {
                payload.Data = ParseJsonElement(graphConfig.Data);
            }

            return payload;
        }

        /// <summary>
        /// Merges saved GlobalFilter with runtime FE filters using AND.
        /// </summary>
        private static FilterGroup? MergeFilters(FilterGroup? saved, FilterGroup? runtime)
        {
            if (runtime == null) return saved;
            if (saved == null) return runtime;

            return new FilterGroup
            {
                Join = JoinOperator.And,
                Rules = new List<FilterRule>(),
                SubGroups = new List<FilterGroup> { saved, runtime }
            };
        }

        /// <summary>
        /// Replaces @-prefixed parameter placeholders with their quoted string values
        /// so the FormQueryEngine (which doesn't support named params) can parse the SQL.
        /// </summary>
        private static string SubstituteParameters(string sql, Dictionary<string, object> parameters)
        {
            foreach (var kv in parameters.OrderByDescending(k => k.Key.Length))
            {
                var key = kv.Key.StartsWith('@') ? kv.Key : $"@{kv.Key}";
                var value = $"'{kv.Value?.ToString()?.Replace("'", "''") ?? ""}'";
                sql = sql.Replace(key, value, StringComparison.OrdinalIgnoreCase);
            }
            return sql;
        }

        /// <summary>
        /// Appends WHERE / ORDER BY / LIMIT clauses to the base FormQuery SQL
        /// based on the effective filter, sort, and limit.
        /// </summary>
        private static string BuildDynamicFormSql(
            string baseSql,
            FilterGroup? filter,
            List<SortRule>? sortRules,
            int? rowLimit)
        {
            // Wrap the base query so we can layer on runtime clauses
            var sql = baseSql.TrimEnd().TrimEnd(';');

            var whereClauses = new List<string>();
            if (filter != null)
            {
                var clause = BuildFilterClause(filter);
                if (!string.IsNullOrWhiteSpace(clause))
                    whereClauses.Add(clause);
            }

            // If the base SQL already has WHERE, we wrap it as a subquery
            if (whereClauses.Count > 0)
            {
                var hasWhere = sql.Contains(" WHERE ", StringComparison.OrdinalIgnoreCase);
                if (hasWhere)
                {
                    sql = $"SELECT * FROM ({sql}) AS _filtered WHERE {string.Join(" AND ", whereClauses)}";
                }
                else
                {
                    sql += $" WHERE {string.Join(" AND ", whereClauses)}";
                }
            }

            if (sortRules != null && sortRules.Count > 0)
            {
                var orderParts = sortRules.Select(s =>
                    $"\"{s.Field}\" {(s.Direction == SortDirection.Desc ? "DESC" : "ASC")}");
                sql += $" ORDER BY {string.Join(", ", orderParts)}";
            }

            if (rowLimit.HasValue && rowLimit.Value > 0)
            {
                sql += $" LIMIT {rowLimit.Value}";
            }

            return sql;
        }

        private static string BuildFilterClause(FilterGroup group)
        {
            var parts = new List<string>();

            foreach (var rule in group.Rules)
            {
                var clause = BuildRuleClause(rule);
                if (!string.IsNullOrWhiteSpace(clause))
                    parts.Add(clause);
            }

            if (group.SubGroups != null)
            {
                foreach (var sub in group.SubGroups)
                {
                    var clause = BuildFilterClause(sub);
                    if (!string.IsNullOrWhiteSpace(clause))
                        parts.Add($"({clause})");
                }
            }

            if (parts.Count == 0) return string.Empty;

            var joiner = group.Join == JoinOperator.Or ? " OR " : " AND ";
            return string.Join(joiner, parts);
        }

        private static string BuildRuleClause(FilterRule rule)
        {
            var field = $"\"{rule.Field}\"";
            var value = rule.Value is string s ? $"'{s.Replace("'", "''")}'" : rule.Value?.ToString() ?? "NULL";

            return rule.Operator switch
            {
                FilterOperator.Eq => $"{field} = {value}",
                FilterOperator.NotEq => $"{field} != {value}",
                FilterOperator.Gt => $"{field} > {value}",
                FilterOperator.Gte => $"{field} >= {value}",
                FilterOperator.Lt => $"{field} < {value}",
                FilterOperator.Lte => $"{field} <= {value}",
                FilterOperator.Like => $"{field} LIKE {value}",
                FilterOperator.IsNull => $"{field} IS NULL",
                FilterOperator.IsNotNull => $"{field} IS NOT NULL",
                FilterOperator.In when rule.Values != null =>
                    $"{field} IN ({string.Join(", ", rule.Values.Select(v => v is string sv ? $"'{sv.Replace("'", "''")}'" : v?.ToString() ?? "NULL"))})",
                FilterOperator.NotIn when rule.Values != null =>
                    $"{field} NOT IN ({string.Join(", ", rule.Values.Select(v => v is string sv ? $"'{sv.Replace("'", "''")}'" : v?.ToString() ?? "NULL"))})",
                _ => string.Empty
            };
        }

        /// <summary>
        /// Transforms tabular query results into GraphDataConfig using series calculations.
        /// </summary>
        private static GraphDataConfig TransformToGraphData(FormQueryResult result, List<SeriesCalculation> seriesCalcs, string? configDataJson = null)
        {
            var graphData = new GraphDataConfig();

            // Try to load labels and series metadata from static config Data JSON
            GraphDataConfig? configData = null;
            if (!string.IsNullOrWhiteSpace(configDataJson))
            {
                try
                {
                    configData = JsonSerializer.Deserialize<GraphDataConfig>(configDataJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    //if (configData?.Labels?.Count > 0)
                    //    graphData.Labels = configData.Labels;
                }
                catch { /* ignore parse errors */ }
            }

            if (seriesCalcs.Count == 0 || result.Rows.Count == 0)
            {
                if (result.Rows.Count > 0 && result.Columns.Count >= 2)
                {
                    var xCol = result.Columns[0];

                    if (configData?.Series != null && configData.Series.Count > 0)
                    {
                        // Number of series determined by config — map each to a result column
                        var yColumns = result.Columns.Where(c => c != xCol).ToList();
                        for (var i = 0; i < configData.Series.Count; i++)
                        {
                            var cfgSeries = configData.Series[i];
                            var yCol = i < yColumns.Count ? yColumns[i] : yColumns.LastOrDefault() ?? result.Columns[1];
                            graphData.Series.Add(new GraphSeries
                            {
                                Name = cfgSeries.Name,
                                Color = cfgSeries.Color,
                                Points = result.Rows.Select(r => new DataPoint
                                {
                                    X = r.GetValueOrDefault(xCol),
                                    Y = r.GetValueOrDefault(yCol) ?? 0
                                }).ToList()
                            });
                        }
                    }
                    else
                    {
                        // No config — single series from first two columns
                        var yCol = result.Columns[1];
                        graphData.Series.Add(new GraphSeries
                        {
                            Name = yCol,
                            Points = result.Rows.Select(r => new DataPoint
                            {
                                X = r.GetValueOrDefault(xCol),
                                Y = r.GetValueOrDefault(yCol) ?? 0
                            }).ToList()
                        });
                    }
                }
                return graphData;
            }

            for (var i = 0; i < seriesCalcs.Count; i++)
            {
                var calc = seriesCalcs[i];
                var rowList = result.Rows.ToList();

                // Use config series name if available, else fall back to calc.SeriesName
                var seriesName = configData?.Series?.ElementAtOrDefault(i)?.Name ?? calc.SeriesName;
                var series = new GraphSeries { Name = seriesName };

                {
                    // No grouping — map rows directly
                    foreach (var row in rowList)
                    {
                        var point = new DataPoint
                        {
                            X = row.GetValueOrDefault(calc.XField),
                            Y = row.GetValueOrDefault(calc.YField) ?? 0
                        };
                        if (calc.ZField != null)
                            point.Z = row.GetValueOrDefault(calc.ZField);

                        series.Points.Add(point);
                    }
                }

                // Extract labels from X values
                if (graphData.Labels.Count == 0)
                    graphData.Labels = series.Points.Select(p => p.X?.ToString() ?? "").ToList();

                graphData.Series.Add(series);
            }

            return graphData;
        }

        private static readonly string[] DefaultColors = new[]
        {
            "#4E79A7", "#F28E2B", "#E15759", "#76B7B2", "#59A14F",
            "#EDC948", "#B07AA1", "#FF9DA7", "#9C755F", "#BAB0AC"
        };

        /// <summary>
        /// Assigns colors to series: first from graphConfig.Data JSON (looks for Series[].Color
        /// or a Colors array), then falls back to a default palette.
        /// </summary>
        private static void AssignSeriesColors(GraphDataConfig graphData, string? configDataJson)
        {
            // Try to extract colors from the saved graphConfig.Data JSON
            var configColors = new List<string>();
            if (!string.IsNullOrWhiteSpace(configDataJson))
            {
                try
                {
                    var dataEl = JsonSerializer.Deserialize<JsonElement>(configDataJson);

                    // Check for a top-level "Colors" array
                    if (dataEl.TryGetProperty("Colors", out var colorsEl) ||
                        dataEl.TryGetProperty("colors", out colorsEl))
                    {
                        if (colorsEl.ValueKind == JsonValueKind.Array)
                            foreach (var c in colorsEl.EnumerateArray())
                                if (c.ValueKind == JsonValueKind.String)
                                    configColors.Add(c.GetString()!);
                    }

                    // Check for colors inside a "Series" array
                    if (configColors.Count == 0 &&
                        (dataEl.TryGetProperty("Series", out var seriesEl) ||
                         dataEl.TryGetProperty("series", out seriesEl)) &&
                        seriesEl.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var s in seriesEl.EnumerateArray())
                        {
                            if ((s.TryGetProperty("Color", out var colorEl) ||
                                 s.TryGetProperty("color", out colorEl)) &&
                                colorEl.ValueKind == JsonValueKind.String)
                                configColors.Add(colorEl.GetString()!);
                        }
                    }
                }
                catch { /* ignore malformed JSON */ }
            }

            for (int i = 0; i < graphData.Series.Count; i++)
            {
                if (string.IsNullOrEmpty(graphData.Series[i].Color))
                {
                    graphData.Series[i].Color = i < configColors.Count
                        ? configColors[i]
                        : DefaultColors[i % DefaultColors.Length];
                }
            }
        }

        // TODO: uncomment when FieldMapping functionality is developed
        // private static object? GetFieldValue(Dictionary<string, object?> row, FieldMapping field)
        // {
        //     if (field.StaticValue != null) return field.StaticValue;
        //     return row.GetValueOrDefault(field.FieldName);
        // }

        // TODO: uncomment when GroupByFields functionality is developed
        // private static object AggregateField(IGrouping<string, Dictionary<string, object?>> group, FieldMapping field)
        // {
        //     if (field.StaticValue != null) return field.StaticValue;
        //     var values = group
        //         .Select(r => r.GetValueOrDefault(field.FieldName))
        //         .Where(v => v != null)
        //         .ToList();
        //     return field.Aggregation switch
        //     {
        //         AggregationType.Count => values.Count,
        //         AggregationType.CountDistinct => values.Distinct().Count(),
        //         AggregationType.Sum => values.Sum(v => Convert.ToDouble(v)),
        //         AggregationType.Avg => values.Average(v => Convert.ToDouble(v)),
        //         AggregationType.Min => values.Min(v => Convert.ToDouble(v)),
        //         AggregationType.Max => values.Max(v => Convert.ToDouble(v)),
        //         _ => values.FirstOrDefault() ?? 0
        //     };
        // }

        private static bool EvaluateFilter(Dictionary<string, object?> row, FilterGroup filter)
        {
            var ruleResults = filter.Rules.Select(r => EvaluateRule(row, r));
            var subResults = filter.SubGroups?.Select(sg => EvaluateFilter(row, sg)) ?? Enumerable.Empty<bool>();
            var all = ruleResults.Concat(subResults);

            return filter.Join == JoinOperator.And ? all.All(b => b) : all.Any(b => b);
        }

        private static bool EvaluateRule(Dictionary<string, object?> row, FilterRule rule)
        {
            var fieldValue = row.GetValueOrDefault(rule.Field);
            var compareValue = rule.Value;

            return rule.Operator switch
            {
                FilterOperator.Eq => Equals(fieldValue?.ToString(), compareValue?.ToString()),
                FilterOperator.NotEq => !Equals(fieldValue?.ToString(), compareValue?.ToString()),
                FilterOperator.IsNull => fieldValue == null,
                FilterOperator.IsNotNull => fieldValue != null,
                FilterOperator.In when rule.Values != null =>
                    rule.Values.Any(v => Equals(fieldValue?.ToString(), v?.ToString())),
                FilterOperator.NotIn when rule.Values != null =>
                    !rule.Values.Any(v => Equals(fieldValue?.ToString(), v?.ToString())),
                FilterOperator.Gt => Compare(fieldValue, compareValue) > 0,
                FilterOperator.Gte => Compare(fieldValue, compareValue) >= 0,
                FilterOperator.Lt => Compare(fieldValue, compareValue) < 0,
                FilterOperator.Lte => Compare(fieldValue, compareValue) <= 0,
                FilterOperator.Like => fieldValue?.ToString()?.Contains(compareValue?.ToString() ?? "", StringComparison.OrdinalIgnoreCase) == true,
                _ => true
            };
        }

        private static int Compare(object? a, object? b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return -1;
            if (b == null) return 1;

            if (double.TryParse(a.ToString(), out var da) && double.TryParse(b.ToString(), out var db))
                return da.CompareTo(db);

            return string.Compare(a.ToString(), b.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private static JsonElement? ParseJsonElement(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return JsonSerializer.Deserialize<JsonElement>(json); }
            catch { return null; }
        }
    }
}