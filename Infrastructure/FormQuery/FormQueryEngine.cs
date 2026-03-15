using System.Data;
using Application.FormQuery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Infrastructure.Persistence;
using Npgsql;

namespace Infrastructure.FormQuery
{
    /// <summary>
    /// Orchestrates the form query pipeline: Parse → Resolve Schema → Translate → Execute.
    /// Caches form schemas in IMemoryCache for performance.
    /// </summary>
    public class FormQueryEngine : IFormQueryEngine
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<FormQueryEngine> _logger;

        private static readonly TimeSpan SchemaCacheDuration = TimeSpan.FromMinutes(10);
        private const string SchemaCachePrefix = "FormSchema_";

        public FormQueryEngine(AppDbContext context, IMemoryCache cache, ILogger<FormQueryEngine> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<FormQueryResult> ExecuteAsync(FormQueryRequest request, Guid? organizationId)
        {
            if (string.IsNullOrWhiteSpace(request.Sql))
                throw new FormQueryException("Query cannot be empty.");

            // 1. Lex
            var lexer = new FormQueryLexer(request.Sql);
            var tokens = lexer.Tokenize();

            // 2. Parse
            var parser = new FormQueryParser(tokens);
            var queryAst = parser.Parse();

            // 3. Resolve schemas for all referenced forms
            var schemas = await ResolveAllSchemasAsync(queryAst, organizationId);

            // 4. Translate to SQL
            var translator = new FormQueryTranslator(schemas, organizationId);
            var (sql, parameters) = translator.Translate(queryAst);

            _logger.LogDebug("Form query translated to SQL: {Sql}", sql);

            // 5. Execute
            return await ExecuteSqlAsync(sql, parameters);
        }

        private async Task<Dictionary<string, FormSchema>> ResolveAllSchemasAsync(
            Application.FormQuery.FormQuery query, Guid? organizationId)
        {
            var schemas = new Dictionary<string, FormSchema>(StringComparer.OrdinalIgnoreCase);

            // Collect all form names
            var formNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { query.From.FormName };
            foreach (var join in query.Joins)
                formNames.Add(join.FormName);

            foreach (var formName in formNames)
            {
                var schema = await GetFormSchemaAsync(formName, organizationId);
                schemas[formName] = schema;

                // Also map by alias if present
                if (query.From.FormName == formName && query.From.Alias != null)
                    schemas[query.From.Alias] = schema;

                foreach (var join in query.Joins)
                {
                    if (join.FormName == formName)
                        schemas[join.Alias] = schema;
                }
            }

            return schemas;
        }

        private async Task<FormSchema> GetFormSchemaAsync(string formName, Guid? organizationId)
        {
            var cacheKey = $"{SchemaCachePrefix}{organizationId}_{formName}";

            if (_cache.TryGetValue(cacheKey, out FormSchema? cached) && cached != null)
                return cached;

            // Load from DB
            var form = await _context.DynamicForms
                .Include(f => f.FieldDefinitions)
                .FirstOrDefaultAsync(f => f.Name == formName && !f.IsDeleted);

            if (form == null)
                throw new FormQueryException($"Form '{formName}' not found.");

            var schema = new FormSchema
            {
                FormId = form.Id,
                FormName = form.Name
            };

            foreach (var field in form.FieldDefinitions.Where(fd => !fd.IsDeleted))
            {
                schema.Fields[field.FieldName] = new FieldSchema
                {
                    FieldDefinitionId = field.Id,
                    FieldName = field.FieldName,
                    ColumnName = field.ColumnName,
                    FieldType = field.FieldType
                };

                // Also allow lookup by ColumnName (e.g., "Field1")
                if (!schema.Fields.ContainsKey(field.ColumnName))
                    schema.Fields[field.ColumnName] = schema.Fields[field.FieldName];
            }

            _cache.Set(cacheKey, schema, SchemaCacheDuration);

            return schema;
        }

        private async Task<FormQueryResult> ExecuteSqlAsync(string sql, List<NpgsqlParameter> parameters)
        {
            var result = new FormQueryResult();

            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.CommandTimeout = 30;

            foreach (var param in parameters)
            {
                var dbParam = command.CreateParameter();
                dbParam.ParameterName = param.ParameterName;
                dbParam.Value = param.Value ?? DBNull.Value;
                command.Parameters.Add(dbParam);
            }

            using var reader = await command.ExecuteReaderAsync();

            // Read column names
            for (int i = 0; i < reader.FieldCount; i++)
            {
                result.Columns.Add(reader.GetName(i));
            }

            // Read rows
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var colName = reader.GetName(i);
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[colName] = value;
                }
                result.Rows.Add(row);
            }

            result.TotalCount = result.Rows.Count;

            return result;
        }
    }
}
