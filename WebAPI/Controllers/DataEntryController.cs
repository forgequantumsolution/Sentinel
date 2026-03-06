using System;
using System.Threading.Tasks;
using Application.Interfaces.Services;
using Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Analytics_BE.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require auth for data entry
    public class DataEntryController : ControllerBase
    {
        private readonly IDataEntryService _dataEntryService;

        public DataEntryController(IDataEntryService dataEntryService)
        {
            _dataEntryService = dataEntryService;
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitData([FromBody] DataEntryRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TableName))
            {
                return BadRequest("Invalid data entry payload. TableName is required.");
            }

            if (request.Data == null || request.Data.Count == 0)
            {
                return BadRequest("No dynamic data fields were provided.");
            }

            try
            {
                var success = await _dataEntryService.InsertDataAsync(request);
                
                if (success)
                    return Ok(new { message = "Data saved successfully" });
                else
                    return BadRequest("Failed to save data. No rows affected.");
            }
            catch (ArgumentException ex)
            {
                // Likely a table access violation based on the whitelist in the service
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // In production, log error and return a generic message to prevent leaking DB structure
                return StatusCode(500, "Error processing dynamic data entry: " + ex.Message);
            }
        }
    }
}
