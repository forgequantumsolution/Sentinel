using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.FormQuery;
using Application.Interfaces;
using Infrastructure.FormQuery;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FormQueryController : ControllerBase
    {
        private readonly IFormQueryEngine _queryEngine;
        private readonly ITenantContext _tenantContext;

        public FormQueryController(IFormQueryEngine queryEngine, ITenantContext tenantContext)
        {
            _queryEngine = queryEngine;
            _tenantContext = tenantContext;
        }

        /// <summary>
        /// Execute a SQL-like query against dynamic forms.
        /// Forms are referenced by name as virtual tables, fields by their display names.
        ///
        /// Examples:
        ///   SELECT "First Name", "Age" FROM "Employee Form" WHERE "Age" > '25'
        ///   SELECT e."Name", d."Department" FROM "Employees" e JOIN "Departments" d ON e."DeptId" = d."Id"
        ///   SELECT COUNT(*), "Status" FROM "Tasks" GROUP BY "Status"
        /// </summary>
        [HttpPost("execute")]
        public async Task<IActionResult> Execute([FromBody] FormQueryRequest request)
        {
            try
            {
                var result = await _queryEngine.ExecuteAsync(request, _tenantContext.OrganizationId);
                return Ok(result);
            }
            catch (FormQueryException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
