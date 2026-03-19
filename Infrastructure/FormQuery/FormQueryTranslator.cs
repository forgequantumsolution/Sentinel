using System.Text;
using Application.FormQuery;
using Npgsql;
using NpgsqlTypes;

namespace Infrastructure.FormQuery
{
    /// <summary>
    /// Translates a FormQuery AST into real PostgreSQL SQL over the EAV table (DynamicFormRecordValues).
    ///
    /// Strategy:
    /// - Each form reference becomes a CTE that pivots EAV rows into columns using conditional aggregation.
    /// - The outer query references CTEs and applies WHERE/ORDER BY/LIMIT etc.
    /// - For single-form queries without JOINs, a simpler inline approach is used.
    /// </summary>
    public class FormQueryTranslator
    {
        private readonly Dictionary<string, FormSchema> _schemas; // alias/name → schema
        private readonly Guid? _organizationId;
        private readonly List<NpgsqlParameter> _parameters = new();
        private int _paramIndex;

        public FormQueryTranslator(Dictionary<string, FormSchema> schemas, Guid? organizationId)
        {
            _schemas = schemas;
            _organizationId = organizationId;
        }

        public (string Sql, List<NpgsqlParameter> Parameters) Translate(Application.FormQuery.FormQuery query)
        {
            _parameters.Clear();
            _paramIndex = 0;

            var sb = new StringBuilder();
            bool hasJoins = query.Joins.Count > 0;

            if (hasJoins)
            {
                TranslateWithCtes(sb, query);
            }
            else
            {
                TranslateSingleForm(sb, query);
            }

            return (sb.ToString(), _parameters);
        }

        // ── Single form (no JOINs) – inline pivot ──

        private void TranslateSingleForm(StringBuilder sb, Application.FormQuery.FormQuery query)
        {
            var schema = ResolveSchema(query.From.FormName);
            var alias = query.From.Alias ?? query.From.FormName;

            // We need to know all fields referenced in the query to include in the pivot
            var referencedFields = CollectReferencedFields(query, alias, schema);

            // Build the query with conditional aggregation
            sb.Append("SELECT ");
            if (query.Select.Distinct) sb.Append("DISTINCT ");

            // SELECT items
            var selectParts = new List<string>();
            foreach (var item in query.Select.Items)
            {
                if (item.Expr is StarExpr)
                {
                    // Select all fields — deduplicate because schema.Fields indexes by both FieldName and ColumnName
                    foreach (var field in schema.Fields.Values.DistinctBy(f => f.FieldDefinitionId))
                    {
                        selectParts.Add($"{PivotExpression(field, schema)} AS \"{field.FieldName}\"");
                    }
                }
                else
                {
                    var sqlExpr = TranslateSelectExpression(item.Expr, alias, schema);
                    var colAlias = item.Alias ?? GetExpressionAlias(item.Expr);
                    selectParts.Add($"{sqlExpr} AS \"{colAlias}\"");
                }
            }
            sb.AppendLine(string.Join(", ", selectParts));

            // FROM
            var formIdParam = AddParameter(schema.FormId);
            sb.AppendLine($"FROM \"DynamicFormRecordValues\" v");
            sb.AppendLine($"INNER JOIN \"DynamicFormSubmissions\" s ON v.\"SubmissionId\" = s.\"Id\"");
            sb.Append($"WHERE v.\"FormId\" = @{formIdParam} AND v.\"IsDeleted\" = false AND s.\"IsDeleted\" = false");

            // Tenant isolation
            if (_organizationId.HasValue)
            {
                var orgParam = AddParameter(_organizationId.Value);
                sb.Append($" AND v.\"OrganizationId\" = @{orgParam}");
            }
            sb.AppendLine();

            // GROUP BY submission
            sb.AppendLine("GROUP BY v.\"SubmissionId\"");

            // WHERE → HAVING (since we're aggregating)
            if (query.Where != null)
            {
                sb.Append("HAVING ");
                sb.AppendLine(TranslateWhereExpression(query.Where, alias, schema));
            }

            // Additional HAVING (from explicit HAVING clause)
            if (query.Having != null)
            {
                sb.Append(query.Where != null ? "AND " : "HAVING ");
                sb.AppendLine(TranslateWhereExpression(query.Having, alias, schema));
            }

            // ORDER BY
            if (query.OrderBy != null)
            {
                var orderParts = query.OrderBy.Select(o =>
                    $"{TranslateExpressionToPivot(o.Expr, alias, schema)} {(o.Descending ? "DESC" : "ASC")}");
                sb.AppendLine($"ORDER BY {string.Join(", ", orderParts)}");
            }

            // LIMIT / OFFSET
            if (query.Limit.HasValue)
            {
                var limitParam = AddParameter(query.Limit.Value);
                sb.AppendLine($"LIMIT @{limitParam}");
            }
            if (query.Offset.HasValue)
            {
                var offsetParam = AddParameter(query.Offset.Value);
                sb.AppendLine($"OFFSET @{offsetParam}");
            }
        }

        // ── Multiple forms with JOINs – CTE based ──

        private void TranslateWithCtes(StringBuilder sb, Application.FormQuery.FormQuery query)
        {
            // Collect all form references: FROM + JOINs
            var formRefs = new List<(string FormName, string Alias)>
            {
                (query.From.FormName, query.From.Alias ?? query.From.FormName)
            };
            foreach (var join in query.Joins)
            {
                formRefs.Add((join.FormName, join.Alias));
            }

            // Build CTEs – one per form, pivoting all referenced fields
            sb.AppendLine("WITH");
            var cteParts = new List<string>();
            foreach (var (formName, formAlias) in formRefs)
            {
                var schema = ResolveSchema(formName);
                var fieldsForForm = CollectReferencedFieldsForAlias(query, formAlias, schema);

                // If no specific fields referenced, include all
                if (fieldsForForm.Count == 0)
                    fieldsForForm = schema.Fields.Values.DistinctBy(f => f.FieldDefinitionId).ToList();

                var cteSb = new StringBuilder();
                var formIdParam = AddParameter(schema.FormId);
                cteSb.Append($"\"{formAlias}\" AS (\n  SELECT v.\"SubmissionId\"");

                foreach (var field in fieldsForForm)
                {
                    cteSb.Append($",\n    {PivotExpression(field, schema)} AS \"{field.FieldName}\"");
                }

                cteSb.Append($"\n  FROM \"DynamicFormRecordValues\" v");
                cteSb.Append($"\n  WHERE v.\"FormId\" = @{formIdParam} AND v.\"IsDeleted\" = false");

                if (_organizationId.HasValue)
                {
                    var orgParam = AddParameter(_organizationId.Value);
                    cteSb.Append($" AND v.\"OrganizationId\" = @{orgParam}");
                }

                cteSb.Append("\n  GROUP BY v.\"SubmissionId\"\n)");
                cteParts.Add(cteSb.ToString());
            }
            sb.AppendLine(string.Join(",\n", cteParts));

            // Outer SELECT
            sb.Append("SELECT ");
            if (query.Select.Distinct) sb.Append("DISTINCT ");

            var selectParts = new List<string>();
            foreach (var item in query.Select.Items)
            {
                if (item.Expr is StarExpr starExpr)
                {
                    var targetAlias = starExpr.TableAlias;
                    if (targetAlias != null)
                    {
                        var schema = ResolveSchemaByAlias(targetAlias, formRefs);
                        foreach (var field in schema.Fields.Values.DistinctBy(f => f.FieldDefinitionId))
                            selectParts.Add($"\"{targetAlias}\".\"{field.FieldName}\"");
                    }
                    else
                    {
                        // All fields from all forms
                        foreach (var (formName, formAlias) in formRefs)
                        {
                            var schema = ResolveSchema(formName);
                            foreach (var field in schema.Fields.Values.DistinctBy(f => f.FieldDefinitionId))
                                selectParts.Add($"\"{formAlias}\".\"{field.FieldName}\"");
                        }
                    }
                }
                else
                {
                    var sqlExpr = TranslateCteExpression(item.Expr);
                    var colAlias = item.Alias ?? GetExpressionAlias(item.Expr);
                    selectParts.Add($"{sqlExpr} AS \"{colAlias}\"");
                }
            }
            sb.AppendLine(string.Join(", ", selectParts));

            // FROM (first CTE)
            var primaryAlias = query.From.Alias ?? query.From.FormName;
            sb.AppendLine($"FROM \"{primaryAlias}\"");

            // JOINs
            foreach (var join in query.Joins)
            {
                var joinKeyword = join.Type switch
                {
                    JoinType.Left => "LEFT JOIN",
                    JoinType.Right => "RIGHT JOIN",
                    _ => "INNER JOIN"
                };
                var onExpr = TranslateCteExpression(join.On);
                sb.AppendLine($"{joinKeyword} \"{join.Alias}\" ON {onExpr}");
            }

            // WHERE
            if (query.Where != null)
            {
                sb.Append("WHERE ");
                sb.AppendLine(TranslateCteWhereExpression(query.Where));
            }

            // GROUP BY
            if (query.GroupBy != null)
            {
                var groupParts = query.GroupBy.Select(TranslateCteExpression);
                sb.AppendLine($"GROUP BY {string.Join(", ", groupParts)}");
            }

            // HAVING
            if (query.Having != null)
            {
                sb.Append("HAVING ");
                sb.AppendLine(TranslateCteWhereExpression(query.Having));
            }

            // ORDER BY
            if (query.OrderBy != null)
            {
                var orderParts = query.OrderBy.Select(o =>
                    $"{TranslateCteExpression(o.Expr)} {(o.Descending ? "DESC" : "ASC")}");
                sb.AppendLine($"ORDER BY {string.Join(", ", orderParts)}");
            }

            // LIMIT / OFFSET
            if (query.Limit.HasValue)
            {
                var limitParam = AddParameter(query.Limit.Value);
                sb.AppendLine($"LIMIT @{limitParam}");
            }
            if (query.Offset.HasValue)
            {
                var offsetParam = AddParameter(query.Offset.Value);
                sb.AppendLine($"OFFSET @{offsetParam}");
            }
        }

        // ── Expression translators for single-form (pivot) mode ──

        private string TranslateSelectExpression(Expression expr, string alias, FormSchema schema)
        {
            return expr switch
            {
                FunctionExpr func => TranslateAggregatePivot(func, alias, schema),
                FieldRefExpr field => PivotExpressionByName(field.FieldName, schema),
                LiteralExpr lit => TranslateLiteral(lit),
                CastExpr cast => $"CAST({TranslateExpressionToPivot(cast.Expr, alias, schema)} AS {MapCastType(cast.TargetType)})",
                CaseExpr caseExpr => TranslateCasePivot(caseExpr, alias, schema),
                _ => TranslateExpressionToPivot(expr, alias, schema)
            };
        }

        private string TranslateExpressionToPivot(Expression expr, string alias, FormSchema schema)
        {
            return expr switch
            {
                FieldRefExpr field => PivotExpressionByName(field.FieldName, schema),
                LiteralExpr lit => TranslateLiteral(lit),
                FunctionExpr func => TranslateAggregatePivot(func, alias, schema),
                BinaryExpr bin => $"({TranslateExpressionToPivot(bin.Left, alias, schema)} {bin.Operator} {TranslateExpressionToPivot(bin.Right, alias, schema)})",
                UnaryExpr un => $"({un.Operator} {TranslateExpressionToPivot(un.Operand, alias, schema)})",
                CastExpr cast => $"CAST({TranslateExpressionToPivot(cast.Expr, alias, schema)} AS {MapCastType(cast.TargetType)})",
                CaseExpr caseExpr => TranslateCasePivot(caseExpr, alias, schema),
                StarExpr => "1",
                _ => throw new FormQueryException($"Unsupported expression type: {expr.GetType().Name}")
            };
        }

        private string TranslateWhereExpression(Expression expr, string alias, FormSchema schema)
        {
            return expr switch
            {
                BinaryExpr bin when bin.Operator is "AND" or "OR" =>
                    $"({TranslateWhereExpression(bin.Left, alias, schema)} {bin.Operator} {TranslateWhereExpression(bin.Right, alias, schema)})",
                BinaryExpr bin =>
                    $"{TranslateExpressionToPivot(bin.Left, alias, schema)} {bin.Operator} {TranslateExpressionToPivot(bin.Right, alias, schema)}",
                UnaryExpr un =>
                    $"{un.Operator} ({TranslateWhereExpression(un.Operand, alias, schema)})",
                IsNullExpr isNull =>
                    $"{TranslateExpressionToPivot(isNull.Expr, alias, schema)} IS {(isNull.Not ? "NOT " : "")}NULL",
                InExpr inExpr =>
                    $"{TranslateExpressionToPivot(inExpr.Expr, alias, schema)} {(inExpr.Not ? "NOT " : "")}IN ({string.Join(", ", inExpr.Values.Select(v => TranslateExpressionToPivot(v, alias, schema)))})",
                BetweenExpr between =>
                    $"{TranslateExpressionToPivot(between.Expr, alias, schema)} {(between.Not ? "NOT " : "")}BETWEEN {TranslateExpressionToPivot(between.Low, alias, schema)} AND {TranslateExpressionToPivot(between.High, alias, schema)}",
                _ => TranslateExpressionToPivot(expr, alias, schema)
            };
        }

        private string TranslateAggregatePivot(FunctionExpr func, string alias, FormSchema schema)
        {
            if (func.Args.Count == 1 && func.Args[0] is StarExpr)
                return $"{func.Name}(*)";

            var innerArgs = func.Args.Select(a => TranslateExpressionToPivot(a, alias, schema));
            var distinct = func.Distinct ? "DISTINCT " : "";
            return $"{func.Name}({distinct}{string.Join(", ", innerArgs)})";
        }

        // ── Expression translators for CTE (join) mode ──

        private string TranslateCteExpression(Expression expr)
        {
            return expr switch
            {
                FieldRefExpr field when field.TableAlias != null =>
                    $"\"{field.TableAlias}\".\"{field.FieldName}\"",
                FieldRefExpr field => $"\"{field.FieldName}\"",
                LiteralExpr lit => TranslateLiteral(lit),
                BinaryExpr bin =>
                    $"({TranslateCteExpression(bin.Left)} {bin.Operator} {TranslateCteExpression(bin.Right)})",
                UnaryExpr un =>
                    $"({un.Operator} {TranslateCteExpression(un.Operand)})",
                FunctionExpr func => TranslateAggregateCte(func),
                CastExpr cast =>
                    $"CAST({TranslateCteExpression(cast.Expr)} AS {MapCastType(cast.TargetType)})",
                CaseExpr caseExpr => TranslateCaseCte(caseExpr),
                IsNullExpr isNull =>
                    $"{TranslateCteExpression(isNull.Expr)} IS {(isNull.Not ? "NOT " : "")}NULL",
                StarExpr star when star.TableAlias != null => $"\"{star.TableAlias}\".*",
                StarExpr => "*",
                _ => throw new FormQueryException($"Unsupported expression type in CTE: {expr.GetType().Name}")
            };
        }

        private string TranslateCteWhereExpression(Expression expr)
        {
            return expr switch
            {
                BinaryExpr bin when bin.Operator is "AND" or "OR" =>
                    $"({TranslateCteWhereExpression(bin.Left)} {bin.Operator} {TranslateCteWhereExpression(bin.Right)})",
                InExpr inExpr =>
                    $"{TranslateCteExpression(inExpr.Expr)} {(inExpr.Not ? "NOT " : "")}IN ({string.Join(", ", inExpr.Values.Select(TranslateCteExpression))})",
                BetweenExpr between =>
                    $"{TranslateCteExpression(between.Expr)} {(between.Not ? "NOT " : "")}BETWEEN {TranslateCteExpression(between.Low)} AND {TranslateCteExpression(between.High)}",
                _ => TranslateCteExpression(expr)
            };
        }

        private string TranslateAggregateCte(FunctionExpr func)
        {
            if (func.Args.Count == 1 && func.Args[0] is StarExpr)
                return $"{func.Name}(*)";

            var distinct = func.Distinct ? "DISTINCT " : "";
            var args = func.Args.Select(TranslateCteExpression);
            return $"{func.Name}({distinct}{string.Join(", ", args)})";
        }

        // ── CASE expression helpers ──

        private string TranslateCasePivot(CaseExpr caseExpr, string alias, FormSchema schema)
        {
            var sb = new StringBuilder("CASE");
            if (caseExpr.Operand != null)
            {
                sb.Append($" {TranslateExpressionToPivot(caseExpr.Operand, alias, schema)}");
            }
            foreach (var when in caseExpr.Whens)
            {
                sb.Append($" WHEN {TranslateExpressionToPivot(when.Condition, alias, schema)}");
                sb.Append($" THEN {TranslateExpressionToPivot(when.Result, alias, schema)}");
            }
            if (caseExpr.Else != null)
            {
                sb.Append($" ELSE {TranslateExpressionToPivot(caseExpr.Else, alias, schema)}");
            }
            sb.Append(" END");
            return sb.ToString();
        }

        private string TranslateCaseCte(CaseExpr caseExpr)
        {
            var sb = new StringBuilder("CASE");
            if (caseExpr.Operand != null)
            {
                sb.Append($" {TranslateCteExpression(caseExpr.Operand)}");
            }
            foreach (var when in caseExpr.Whens)
            {
                sb.Append($" WHEN {TranslateCteExpression(when.Condition)}");
                sb.Append($" THEN {TranslateCteExpression(when.Result)}");
            }
            if (caseExpr.Else != null)
            {
                sb.Append($" ELSE {TranslateCteExpression(caseExpr.Else)}");
            }
            sb.Append(" END");
            return sb.ToString();
        }

        // ── Pivot helpers ──

        private string PivotExpression(FieldSchema field, FormSchema schema)
        {
            var paramName = AddParameter(field.FieldDefinitionId);
            return $"MAX(CASE WHEN v.\"FieldDefinitionId\" = @{paramName} THEN v.\"Value\" END)";
        }

        private string PivotExpressionByName(string fieldName, FormSchema schema)
        {
            if (!schema.Fields.TryGetValue(fieldName, out var field))
                throw new FormQueryException($"Field '{fieldName}' not found in form '{schema.FormName}'");

            var paramName = AddParameter(field.FieldDefinitionId);
            return $"MAX(CASE WHEN v.\"FieldDefinitionId\" = @{paramName} THEN v.\"Value\" END)";
        }

        // ── Shared helpers ──

        private string TranslateLiteral(LiteralExpr lit)
        {
            if (lit.Type == LiteralType.Null) return "NULL";
            if (lit.Type == LiteralType.Boolean) return (bool)lit.Value! ? "TRUE" : "FALSE";
            var paramName = AddParameter(lit.Value!);
            return $"@{paramName}";
        }

        private string AddParameter(object value)
        {
            var name = $"p{_paramIndex++}";
            _parameters.Add(new NpgsqlParameter(name, value));
            return name;
        }

        private static string MapCastType(string type) => type.ToUpperInvariant() switch
        {
            "INT" or "INTEGER" => "INTEGER",
            "DECIMAL" or "NUMERIC" => "NUMERIC",
            "FLOAT" or "DOUBLE" => "DOUBLE PRECISION",
            "BOOL" or "BOOLEAN" => "BOOLEAN",
            "DATE" => "DATE",
            "DATETIME" or "TIMESTAMP" => "TIMESTAMP",
            "TEXT" or "STRING" or "VARCHAR" => "TEXT",
            _ => "TEXT"
        };

        private FormSchema ResolveSchema(string formName)
        {
            // Try by name first, then by alias
            if (_schemas.TryGetValue(formName, out var schema))
                return schema;
            throw new FormQueryException($"Form '{formName}' not found. Ensure the form exists and you have access.");
        }

        private FormSchema ResolveSchemaByAlias(string alias, List<(string FormName, string Alias)> formRefs)
        {
            var formRef = formRefs.FirstOrDefault(f => f.Alias == alias);
            if (formRef == default)
                throw new FormQueryException($"Unknown table alias '{alias}'");
            return ResolveSchema(formRef.FormName);
        }

        private string GetExpressionAlias(Expression expr) => expr switch
        {
            FieldRefExpr f => f.FieldName,
            FunctionExpr f => $"{f.Name}_{(f.Args.FirstOrDefault() is FieldRefExpr fr ? fr.FieldName : "expr")}",
            _ => "expr"
        };

        // ── Field collection ──

        private List<FieldSchema> CollectReferencedFields(Application.FormQuery.FormQuery query, string alias, FormSchema schema)
        {
            var fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CollectFieldsFromExpressions(query, alias, fields);

            if (fields.Count == 0)
                return schema.Fields.Values.DistinctBy(f => f.FieldDefinitionId).ToList();

            return fields
                .Where(f => schema.Fields.ContainsKey(f))
                .Select(f => schema.Fields[f])
                .DistinctBy(f => f.FieldDefinitionId)
                .ToList();
        }

        private List<FieldSchema> CollectReferencedFieldsForAlias(Application.FormQuery.FormQuery query, string alias, FormSchema schema)
        {
            var fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Collect from SELECT
            foreach (var item in query.Select.Items)
                CollectFieldNamesFromExpression(item.Expr, alias, fields);

            // Collect from WHERE
            if (query.Where != null)
                CollectFieldNamesFromExpression(query.Where, alias, fields);

            // Collect from JOINs ON
            foreach (var join in query.Joins)
            {
                if (join.Alias == alias)
                    CollectFieldNamesFromExpression(join.On, alias, fields);
                else
                    CollectFieldNamesFromExpression(join.On, alias, fields);
            }

            // Collect from ORDER BY, GROUP By, Having
            if (query.OrderBy != null)
                foreach (var o in query.OrderBy) CollectFieldNamesFromExpression(o.Expr, alias, fields);
            if (query.GroupBy != null)
                foreach (var g in query.GroupBy) CollectFieldNamesFromExpression(g, alias, fields);
            if (query.Having != null)
                CollectFieldNamesFromExpression(query.Having, alias, fields);

            if (fields.Count == 0)
                return schema.Fields.Values.ToList();

            return fields
                .Where(f => schema.Fields.ContainsKey(f))
                .Select(f => schema.Fields[f])
                .ToList();
        }

        private void CollectFieldsFromExpressions(Application.FormQuery.FormQuery query, string alias, HashSet<string> fields)
        {
            foreach (var item in query.Select.Items)
                CollectFieldNamesFromExpression(item.Expr, null, fields);
            if (query.Where != null)
                CollectFieldNamesFromExpression(query.Where, null, fields);
            if (query.OrderBy != null)
                foreach (var o in query.OrderBy) CollectFieldNamesFromExpression(o.Expr, null, fields);
            if (query.GroupBy != null)
                foreach (var g in query.GroupBy) CollectFieldNamesFromExpression(g, null, fields);
            if (query.Having != null)
                CollectFieldNamesFromExpression(query.Having, null, fields);
        }

        private void CollectFieldNamesFromExpression(Expression expr, string? targetAlias, HashSet<string> fields)
        {
            switch (expr)
            {
                case FieldRefExpr f when targetAlias == null || f.TableAlias == null || f.TableAlias == targetAlias:
                    fields.Add(f.FieldName);
                    break;
                case BinaryExpr bin:
                    CollectFieldNamesFromExpression(bin.Left, targetAlias, fields);
                    CollectFieldNamesFromExpression(bin.Right, targetAlias, fields);
                    break;
                case UnaryExpr un:
                    CollectFieldNamesFromExpression(un.Operand, targetAlias, fields);
                    break;
                case FunctionExpr func:
                    foreach (var arg in func.Args) CollectFieldNamesFromExpression(arg, targetAlias, fields);
                    break;
                case InExpr inExpr:
                    CollectFieldNamesFromExpression(inExpr.Expr, targetAlias, fields);
                    foreach (var v in inExpr.Values) CollectFieldNamesFromExpression(v, targetAlias, fields);
                    break;
                case BetweenExpr between:
                    CollectFieldNamesFromExpression(between.Expr, targetAlias, fields);
                    CollectFieldNamesFromExpression(between.Low, targetAlias, fields);
                    CollectFieldNamesFromExpression(between.High, targetAlias, fields);
                    break;
                case IsNullExpr isNull:
                    CollectFieldNamesFromExpression(isNull.Expr, targetAlias, fields);
                    break;
                case CastExpr cast:
                    CollectFieldNamesFromExpression(cast.Expr, targetAlias, fields);
                    break;
                case CaseExpr caseExpr:
                    if (caseExpr.Operand != null)
                        CollectFieldNamesFromExpression(caseExpr.Operand, targetAlias, fields);
                    foreach (var when in caseExpr.Whens)
                    {
                        CollectFieldNamesFromExpression(when.Condition, targetAlias, fields);
                        CollectFieldNamesFromExpression(when.Result, targetAlias, fields);
                    }
                    if (caseExpr.Else != null)
                        CollectFieldNamesFromExpression(caseExpr.Else, targetAlias, fields);
                    break;
                case StarExpr:
                    break; // Star means all fields — handled separately
            }
        }
    }
}
