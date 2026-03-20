using System.Text;
using Application.FormQuery;
using Npgsql;

namespace Infrastructure.FormQuery
{
    /// <summary>
    /// Translates a FormQuery AST into PostgreSQL SQL over the EAV table (DynamicFormRecordValues).
    ///
    /// Strategy (always CTE-based):
    /// 1. Each form becomes a CTE that pivots EAV rows into columns via MAX(CASE...) grouped by SubmissionId.
    /// 2. The outer query references CTEs as regular tables and applies SELECT/WHERE/ORDER BY/etc.
    /// </summary>
    public class FormQueryTranslator
    {
        private readonly Dictionary<string, FormSchema> _schemas;
        private readonly Guid? _organizationId;
        private readonly List<NpgsqlParameter> _parameters = new();
        private int _paramIndex;

        // Tracks all form references (FROM + JOINs) so outer expressions can resolve aliases
        private List<(string FormName, string Alias)> _formRefs = new();

        public FormQueryTranslator(Dictionary<string, FormSchema> schemas, Guid? organizationId)
        {
            _schemas = schemas;
            _organizationId = organizationId;
        }

        public (string Sql, List<NpgsqlParameter> Parameters) Translate(Application.FormQuery.FormQuery query)
        {
            _parameters.Clear();
            _paramIndex = 0;

            // Collect all form references
            var primaryAlias = query.From.Alias ?? query.From.FormName;
            _formRefs = new List<(string FormName, string Alias)>
            {
                (query.From.FormName, primaryAlias)
            };
            foreach (var join in query.Joins)
            {
                _formRefs.Add((join.FormName, join.Alias));
            }

            var sb = new StringBuilder();

            // 1. Build CTEs
            BuildCtes(sb, query);

            // 2. Build outer query
            BuildOuterQuery(sb, query, primaryAlias);

            return (sb.ToString(), _parameters);
        }

        // ── Step 1: Build CTEs ──

        private void BuildCtes(StringBuilder sb, Application.FormQuery.FormQuery query)
        {
            sb.AppendLine("WITH");
            var cteParts = new List<string>();

            foreach (var (formName, formAlias) in _formRefs)
            {
                var schema = ResolveSchema(formName);
                var fields = CollectReferencedFieldsForAlias(query, formAlias, schema);

                if (fields.Count == 0)
                    fields = schema.Fields.Values.DistinctBy(f => f.FieldDefinitionId).ToList();

                var cteSb = new StringBuilder();
                var formIdParam = AddParameter(schema.FormId);

                cteSb.Append($"\"{formAlias}\" AS (\n  SELECT v.\"SubmissionId\"");

                foreach (var field in fields)
                {
                    var paramName = AddParameter(field.FieldDefinitionId);
                    var cast = MapFieldTypeToPgCast(field.FieldType);
                    cteSb.Append($",\n    (MAX(CASE WHEN v.\"FieldDefinitionId\" = @{paramName} THEN v.\"Value\" END)){cast} AS \"{field.FieldName}\"");
                }

                cteSb.Append($"\n  FROM \"DynamicFormRecordValues\" v");
                cteSb.Append($"\n  INNER JOIN \"DynamicFormSubmissions\" s ON v.\"SubmissionId\" = s.\"Id\"");
                cteSb.Append($"\n  WHERE v.\"FormId\" = @{formIdParam} AND v.\"IsDeleted\" = false AND s.\"IsDeleted\" = false");

                if (_organizationId.HasValue)
                {
                    var orgParam = AddParameter(_organizationId.Value);
                    cteSb.Append($" AND v.\"OrganizationId\" = @{orgParam}");
                }

                cteSb.Append("\n  GROUP BY v.\"SubmissionId\"\n)");
                cteParts.Add(cteSb.ToString());
            }

            sb.AppendLine(string.Join(",\n", cteParts));
        }

        // ── Step 2: Build outer query over CTEs ──

        private void BuildOuterQuery(StringBuilder sb, Application.FormQuery.FormQuery query, string primaryAlias)
        {
            // SELECT
            sb.Append("SELECT ");
            if (query.Select.Distinct) sb.Append("DISTINCT ");

            var selectParts = new List<string>();
            foreach (var item in query.Select.Items)
            {
                if (item.Expr is StarExpr starExpr)
                {
                    ExpandStar(starExpr, selectParts);
                }
                else
                {
                    var sqlExpr = TranslateExpression(item.Expr);
                    var colAlias = item.Alias ?? GetExpressionAlias(item.Expr);
                    selectParts.Add($"{sqlExpr} AS \"{colAlias}\"");
                }
            }
            sb.AppendLine(string.Join(", ", selectParts));

            // FROM
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
                var onExpr = TranslateExpression(join.On);
                sb.AppendLine($"{joinKeyword} \"{join.Alias}\" ON {onExpr}");
            }

            // WHERE
            if (query.Where != null)
            {
                sb.Append("WHERE ");
                sb.AppendLine(TranslateWhereExpression(query.Where));
            }

            // GROUP BY
            if (query.GroupBy != null)
            {
                var groupParts = query.GroupBy.Select(TranslateExpression);
                sb.AppendLine($"GROUP BY {string.Join(", ", groupParts)}");
            }

            // HAVING
            if (query.Having != null)
            {
                sb.Append("HAVING ");
                sb.AppendLine(TranslateWhereExpression(query.Having));
            }

            // ORDER BY
            if (query.OrderBy != null)
            {
                var orderParts = query.OrderBy.Select(o =>
                    $"{TranslateExpression(o.Expr)} {(o.Descending ? "DESC" : "ASC")}");
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

        // ── Star expansion ──

        private void ExpandStar(StarExpr starExpr, List<string> selectParts)
        {
            if (starExpr.TableAlias != null)
            {
                var schema = ResolveSchemaByAlias(starExpr.TableAlias);
                foreach (var field in schema.Fields.Values.DistinctBy(f => f.FieldDefinitionId))
                    selectParts.Add($"\"{starExpr.TableAlias}\".\"{field.FieldName}\"");
            }
            else
            {
                foreach (var (formName, formAlias) in _formRefs)
                {
                    var schema = ResolveSchema(formName);
                    foreach (var field in schema.Fields.Values.DistinctBy(f => f.FieldDefinitionId))
                        selectParts.Add($"\"{formAlias}\".\"{field.FieldName}\"");
                }
            }
        }

        // ── Outer expression translators (operate on CTE columns) ──

        private string TranslateExpression(Expression expr)
        {
            return expr switch
            {
                FieldRefExpr field when field.TableAlias != null =>
                    $"\"{field.TableAlias}\".\"{field.FieldName}\"",
                FieldRefExpr field => ResolveFieldColumn(field.FieldName),
                LiteralExpr lit => TranslateLiteral(lit),
                BinaryExpr bin =>
                    $"({TranslateExpression(bin.Left)} {bin.Operator} {TranslateExpression(bin.Right)})",
                UnaryExpr un =>
                    $"({un.Operator} {TranslateExpression(un.Operand)})",
                FunctionExpr func => TranslateFunction(func),
                CastExpr cast =>
                    $"CAST({TranslateExpression(cast.Expr)} AS {MapCastType(cast.TargetType)})",
                CaseExpr caseExpr => TranslateCase(caseExpr),
                IsNullExpr isNull =>
                    $"{TranslateExpression(isNull.Expr)} IS {(isNull.Not ? "NOT " : "")}NULL",
                StarExpr star when star.TableAlias != null => $"\"{star.TableAlias}\".*",
                StarExpr => "*",
                _ => throw new FormQueryException($"Unsupported expression type: {expr.GetType().Name}")
            };
        }

        private string TranslateWhereExpression(Expression expr)
        {
            return expr switch
            {
                BinaryExpr bin when bin.Operator is "AND" or "OR" =>
                    $"({TranslateWhereExpression(bin.Left)} {bin.Operator} {TranslateWhereExpression(bin.Right)})",
                InExpr inExpr =>
                    $"{TranslateExpression(inExpr.Expr)} {(inExpr.Not ? "NOT " : "")}IN ({string.Join(", ", inExpr.Values.Select(TranslateExpression))})",
                BetweenExpr between =>
                    $"{TranslateExpression(between.Expr)} {(between.Not ? "NOT " : "")}BETWEEN {TranslateExpression(between.Low)} AND {TranslateExpression(between.High)}",
                _ => TranslateExpression(expr)
            };
        }

        private string TranslateFunction(FunctionExpr func)
        {
            if (func.Args.Count == 1 && func.Args[0] is StarExpr)
                return $"{func.Name}(*)";

            // Translate SQL Server functions to PostgreSQL equivalents
            var translated = TranslateFunctionToPostgres(func);
            if (translated != null)
                return translated;

            var distinct = func.Distinct ? "DISTINCT " : "";
            var args = func.Args.Select(TranslateExpression);
            return $"{func.Name}({distinct}{string.Join(", ", args)})";
        }

        /// <summary>
        /// Converts SQL Server-style functions to PostgreSQL equivalents.
        /// Returns null if no translation is needed.
        /// </summary>
        private string? TranslateFunctionToPostgres(FunctionExpr func)
        {
            switch (func.Name)
            {
                // FORMAT(date, 'pattern') → TO_CHAR(date, pg_pattern)
                case "FORMAT" when func.Args.Count == 2:
                    var formatExpr = TranslateExpression(func.Args[0]);
                    var pattern = func.Args[1] is LiteralExpr lit && lit.Type == LiteralType.String
                        ? TranslateDateFormat((string)lit.Value!)
                        : TranslateExpression(func.Args[1]);
                    return $"TO_CHAR({formatExpr}, {pattern})";

                // ISNULL(expr, default) → COALESCE(expr, default)
                case "ISNULL" when func.Args.Count == 2:
                    return $"COALESCE({TranslateExpression(func.Args[0])}, {TranslateExpression(func.Args[1])})";

                // LEN(str) → LENGTH(str)
                case "LEN" when func.Args.Count == 1:
                    return $"LENGTH({TranslateExpression(func.Args[0])})";

                // GETDATE() / GETUTCDATE() → NOW() / NOW() AT TIME ZONE 'UTC'
                case "GETDATE" when func.Args.Count == 0:
                    return "NOW()";
                case "GETUTCDATE" when func.Args.Count == 0:
                    return "(NOW() AT TIME ZONE 'UTC')";

                // DATEPART(part, date) → EXTRACT(part FROM date)
                case "DATEPART" when func.Args.Count == 2:
                    var part = func.Args[0] is FieldRefExpr partRef ? partRef.FieldName : TranslateExpression(func.Args[0]);
                    return $"EXTRACT({part} FROM {TranslateExpression(func.Args[1])})";

                // DATEDIFF(part, start, end) → EXTRACT(EPOCH FROM (end - start)) based on part
                case "DATEDIFF" when func.Args.Count == 3:
                    var diffPart = func.Args[0] is FieldRefExpr diffPartRef ? diffPartRef.FieldName.ToUpperInvariant() : "DAY";
                    var startExpr = TranslateExpression(func.Args[1]);
                    var endExpr = TranslateExpression(func.Args[2]);
                    return diffPart switch
                    {
                        "SECOND" => $"EXTRACT(EPOCH FROM ({endExpr} - {startExpr}))::INTEGER",
                        "MINUTE" => $"(EXTRACT(EPOCH FROM ({endExpr} - {startExpr})) / 60)::INTEGER",
                        "HOUR" => $"(EXTRACT(EPOCH FROM ({endExpr} - {startExpr})) / 3600)::INTEGER",
                        _ => $"(({endExpr}::DATE - {startExpr}::DATE))::INTEGER" // DAY default
                    };

                default:
                    return null;
            }
        }

        /// <summary>
        /// Converts .NET/SQL Server date format patterns to PostgreSQL TO_CHAR patterns.
        /// </summary>
        private string TranslateDateFormat(string dotnetFormat)
        {
            var pgFormat = dotnetFormat
                .Replace("yyyy", "YYYY")
                .Replace("yy", "YY")
                .Replace("MMMM", "Month")
                .Replace("MMM", "Mon")
                .Replace("MM", "MM")
                .Replace("dd", "DD")
                .Replace("HH", "HH24")
                .Replace("hh", "HH12")
                .Replace("mm", "MI")
                .Replace("ss", "SS")
                .Replace("tt", "AM");

            var paramName = AddParameter(pgFormat, dedup: true);
            return $"@{paramName}";
        }

        private string TranslateCase(CaseExpr caseExpr)
        {
            var sb = new StringBuilder("CASE");
            if (caseExpr.Operand != null)
                sb.Append($" {TranslateExpression(caseExpr.Operand)}");
            foreach (var when in caseExpr.Whens)
            {
                sb.Append($" WHEN {TranslateExpression(when.Condition)}");
                sb.Append($" THEN {TranslateExpression(when.Result)}");
            }
            if (caseExpr.Else != null)
                sb.Append($" ELSE {TranslateExpression(caseExpr.Else)}");
            sb.Append(" END");
            return sb.ToString();
        }

        // ── Field resolution ──

        /// <summary>
        /// For unqualified field references, find which CTE contains the field and qualify it.
        /// If only one form, use unqualified column name.
        /// </summary>
        private string ResolveFieldColumn(string fieldName)
        {
            if (_formRefs.Count == 1)
                return $"\"{fieldName}\"";

            // Find which form has this field
            foreach (var (formName, formAlias) in _formRefs)
            {
                var schema = ResolveSchema(formName);
                if (schema.Fields.ContainsKey(fieldName))
                    return $"\"{formAlias}\".\"{fieldName}\"";
            }

            // Fallback — let PostgreSQL resolve it
            return $"\"{fieldName}\"";
        }

        // ── Shared helpers ──

        private string TranslateLiteral(LiteralExpr lit)
        {
            if (lit.Type == LiteralType.Null) return "NULL";
            if (lit.Type == LiteralType.Boolean) return (bool)lit.Value! ? "TRUE" : "FALSE";

            var paramName = AddParameter(lit.Value!, dedup: true);

            // Auto-cast date-like strings so PostgreSQL can compare them with DATE/TIMESTAMP columns
            if (lit.Type == LiteralType.String && IsDateLikeString((string)lit.Value!))
                return $"@{paramName}::DATE";

            return $"@{paramName}";
        }

        private static bool IsDateLikeString(string value)
        {
            return DateTime.TryParse(value, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out _);
        }

        /// <summary>
        /// Adds a SQL parameter. When dedup is true, reuses an existing parameter with the same value
        /// so that PostgreSQL sees identical expressions in SELECT and GROUP BY as the same expression.
        /// CTE-internal parameters (FormId, FieldDefinitionId, OrgId) use dedup=false (default)
        /// to avoid cross-CTE conflicts.
        /// </summary>
        private string AddParameter(object value, bool dedup = false)
        {
            if (dedup)
            {
                var existing = _parameters.FirstOrDefault(p => Equals(p.Value, value));
                if (existing != null)
                    return existing.ParameterName;
            }

            var name = $"p{_paramIndex++}";
            _parameters.Add(new NpgsqlParameter(name, value));
            return name;
        }

        private static string MapFieldTypeToPgCast(string fieldType) => fieldType.ToUpperInvariant() switch
        {
            "INT" or "INTEGER" or "NUMBER" => "::INTEGER",
            "DECIMAL" or "FLOAT" or "DOUBLE" => "::NUMERIC",
            "BOOLEAN" or "BOOL" => "::BOOLEAN",
            "DATETIME" or "TIMESTAMP" => "::TIMESTAMP",
            "DATE" => "::DATE",
            _ => "" // String / Text — no cast needed
        };

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
            if (_schemas.TryGetValue(formName, out var schema))
                return schema;
            throw new FormQueryException($"Form '{formName}' not found. Ensure the form exists and you have access.");
        }

        private FormSchema ResolveSchemaByAlias(string alias)
        {
            var formRef = _formRefs.FirstOrDefault(f => f.Alias == alias);
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

        // ── Field collection (determines which fields to pivot in each CTE) ──

        private List<FieldSchema> CollectReferencedFieldsForAlias(Application.FormQuery.FormQuery query, string alias, FormSchema schema)
        {
            // If SELECT * (unqualified or targeting this alias), include all fields
            foreach (var item in query.Select.Items)
            {
                if (item.Expr is StarExpr star && (star.TableAlias == null || star.TableAlias == alias))
                    return schema.Fields.Values.DistinctBy(f => f.FieldDefinitionId).ToList();
            }

            var fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in query.Select.Items)
                CollectFieldNamesFromExpression(item.Expr, alias, fields);
            if (query.Where != null)
                CollectFieldNamesFromExpression(query.Where, alias, fields);
            foreach (var join in query.Joins)
                CollectFieldNamesFromExpression(join.On, alias, fields);
            if (query.OrderBy != null)
                foreach (var o in query.OrderBy) CollectFieldNamesFromExpression(o.Expr, alias, fields);
            if (query.GroupBy != null)
                foreach (var g in query.GroupBy) CollectFieldNamesFromExpression(g, alias, fields);
            if (query.Having != null)
                CollectFieldNamesFromExpression(query.Having, alias, fields);

            if (fields.Count == 0)
                return schema.Fields.Values.DistinctBy(f => f.FieldDefinitionId).ToList();

            return fields
                .Where(f => schema.Fields.ContainsKey(f))
                .Select(f => schema.Fields[f])
                .DistinctBy(f => f.FieldDefinitionId)
                .ToList();
        }

        private void CollectFieldNamesFromExpression(Expression expr, string targetAlias, HashSet<string> fields)
        {
            switch (expr)
            {
                case FieldRefExpr f when f.TableAlias == null || f.TableAlias == targetAlias:
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
                    break;
            }
        }
    }
}
