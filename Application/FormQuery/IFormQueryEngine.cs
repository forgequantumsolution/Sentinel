namespace Application.FormQuery
{
    public interface IFormQueryEngine
    {
        /// <summary>
        /// Executes a form-SQL query and returns tabular results.
        /// </summary>
        Task<FormQueryResult> ExecuteAsync(FormQueryRequest request, Guid? organizationId);
    }

    public class FormQueryRequest
    {
        /// <summary>
        /// SQL-like query using form names as tables and field names as columns.
        /// e.g. SELECT "First Name", "Age" FROM "Employee Form" WHERE "Age" > 25
        /// </summary>
        public string Sql { get; set; } = string.Empty;
    }

    public class FormQueryResult
    {
        public List<string> Columns { get; set; } = new();
        public List<Dictionary<string, object?>> Rows { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
