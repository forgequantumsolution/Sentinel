using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.DTOs;
using Application.Common.Pagination;
using Application.Interfaces.Services;
using System.Security.Claims;

namespace Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppFoldersController : ControllerBase
    {
        private readonly IAppFolderService _folderService;

        public AppFoldersController(IAppFolderService folderService)
        {
            _folderService = folderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PageRequest pageRequest)
        {
            var result = await _folderService.GetAllTreeAsync(pageRequest);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var folder = await _folderService.GetByIdAsync(id);
            if (folder == null) return NotFound();
            return Ok(folder);
        }

        [HttpGet("by-route")]
        public async Task<IActionResult> GetByRoute([FromQuery] string route)
        {
            var folder = await _folderService.GetByRouteAsync(route);
            if (folder == null) return NotFound();
            return Ok(folder);
        }

        [HttpPost]
        [Authorize(Roles = "sys-admin")]
        public async Task<IActionResult> Create([FromBody] CreateAppFolderDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid? userId = Guid.TryParse(userIdClaim, out var parsed) ? parsed : null;

            try
            {
                var result = await _folderService.CreateAsync(dto, userId);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "sys-admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAppFolderDto dto)
        {
            try
            {
                var result = await _folderService.UpdateAsync(id, dto);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "sys-admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _folderService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Move an ActionObject into this folder (sets its ParentObjectId).
        /// </summary>
        [HttpPost("{folderId}/children")]
        [Authorize(Roles = "sys-admin")]
        public async Task<IActionResult> MoveToFolder(Guid folderId, [FromBody] MoveToFolderDto dto)
        {
            var moved = await _folderService.MoveToFolderAsync(folderId, dto.ActionObjectId);
            if (!moved) return NotFound();
            return Ok();
        }

        /// <summary>
        /// Remove an ActionObject from its parent folder.
        /// </summary>
        [HttpDelete("children/{actionObjectId}")]
        [Authorize(Roles = "sys-admin")]
        public async Task<IActionResult> RemoveFromFolder(Guid actionObjectId)
        {
            var removed = await _folderService.RemoveFromFolderAsync(actionObjectId);
            if (!removed) return NotFound();
            return NoContent();
        }
    }
}
