using Bureau;
using Microsoft.AspNetCore.Mvc;
using Niles.Chores.Api.Dtos;
using Niles.Chores.Api.Utilities;
using Niles.Chores.Services;

namespace Niles.Chores.Api.Controllers
{
    [Route("api/chores/{id}/critical")]
    [ApiController]
    public class CriticalChoresController : ControllerBase
    {

        private readonly ICriticalChoreService _criticalChoreService;

        public CriticalChoresController(ICriticalChoreService criticalChoreService)
        {
            _criticalChoreService = criticalChoreService;
        }

        [HttpPost]
        public async Task<IActionResult> MarkCriticalAsync(string id, [FromBody] CriticalChoreDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Decode chore ID
            if (!IdObfuscator.TryDecode(id, out int choreId))
            {
                return BadRequest(new { error = ErrorMessages.InvalidIdFormat });
            }

            // Create critical chore
            Result result = await _criticalChoreService.CreateCriticalChoreAsync(choreId, dto.Description, cancellationToken);
            if (!result.IsSuccess)
            {
                if (result.Error.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(new { error = result.Error.ErrorMessage });
                }
                if (result.Error.ErrorMessage?.Contains("already has an open critical status") == true)
                {
                    return BadRequest(new { error = result.Error.ErrorMessage });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error.ErrorMessage ?? "Failed to mark chore as critical." });
            }

            return Ok(new { message = "Chore marked as critical successfully" });
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveCriticalAsync(string id, CancellationToken cancellationToken)
        {
            // Decode chore ID
            if (!IdObfuscator.TryDecode(id, out int choreId))
            {
                return BadRequest(new { error = ErrorMessages.InvalidIdFormat });
            }

            // Delete critical chore
            Result result = await _criticalChoreService.DeleteCriticalChoreAsync(choreId, cancellationToken);
            if (!result.IsSuccess)
            {
                if (result.Error.ErrorMessage?.Contains("No open critical status") == true)
                {
                    return BadRequest(new { error = result.Error.ErrorMessage });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error.ErrorMessage ?? "Failed to remove critical status." });
            }

            return Ok(new { message = "Critical status removed successfully" });
        }
    }
}
