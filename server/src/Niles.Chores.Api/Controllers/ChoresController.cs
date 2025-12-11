using Bureau;
using Microsoft.AspNetCore.Mvc;
using Niles.Chores.Api.Dtos;
using Niles.Chores.Api.Factories;
using Niles.Chores.Api.Mappers;
using Niles.Chores.Api.Utilities;
using Niles.Chores.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Niles.Chores.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChoresController : ControllerBase
    {
        private readonly IChoreService _choreService;
        private readonly ICriticalChoreService _criticalChoreService;

        public ChoresController(IChoreService choreService, IHouseKeepingService housekeepingService, ICriticalChoreService criticalChoreService)
        {
            _choreService = choreService;
            _criticalChoreService = criticalChoreService;
        }

        // GET: api/chores?search=term&page=1&pageSize=10 (returns paginated chores with search)
        // GET: api/chores (returns first page)
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] SearchQueryDto queryParams, CancellationToken cancellationToken)
        {
            SearchParameters searchParameters = SearchParametersFactory.Create(queryParams);

            PagedResult<Chore> pagedResult = await _choreService.GetChoresAsync(searchParameters, cancellationToken);

            return Ok(pagedResult.ToPagedResponse(x => x.ToDto()));
        }

        // GET api/chores/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            if (!IdObfuscator.TryDecode(id, out int intId))
            {
                return BadRequest(new { error = "Invalid id format." });
            }
            Result<Chore> result = await _choreService.GetChoreAsync(intId, cancellationToken);
            if (result.IsError)
            {
                return NotFound();
            }
            return Ok(result.Value.ToDto());
        }

        // POST api/chores
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] ChoreDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Result<Chore> resultModel = dto.ToResultModel();
            if (!resultModel.IsSuccess)
            {
                return BadRequest(new { error = resultModel.Error.ErrorMessage });
            }

            Result<Chore> result = await _choreService.CreateChoreAsync(resultModel.Value!, cancellationToken);
            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error.ErrorMessage ?? "Failed to create chore." });
            }

            string encodedId = IdObfuscator.Encode(result.Value!.Id);
            dto.Id = encodedId;
            return CreatedAtAction(nameof(GetAsync), new { id = encodedId }, dto);
        }

        // PUT api/chores/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(string id, [FromBody] ChoreDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.Id != null && dto.Id != id)
            {
                return BadRequest(new { error = "Id in body does not match route id." });
            }
            if (!IdObfuscator.TryDecode(id, out int intId))
            {
                return BadRequest(new { error = "Invalid id format." });
            }

            Result<Chore> modelResult = dto.ToResultModel(intId);

            if (!modelResult.IsSuccess)
            {
                return BadRequest(new { error = modelResult.Error.ErrorMessage });
            }

            Result<Chore> result = await _choreService.UpdateChoreAsync(modelResult.Value!, cancellationToken);
            if (!result.IsSuccess)
            {
                if (result.Error.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound();
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error.ErrorMessage ?? "Failed to update chore." });
            }
            return NoContent();
        }

        // DELETE api/chores/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            if (!IdObfuscator.TryDecode(id, out int intId))
            {
                return BadRequest(new { error = "Invalid id format." });
            }

            Result result = await _choreService.DeleteChoreAsync(intId, cancellationToken);
            if (!result.IsSuccess)
            {
                if (result.Error.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound();
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error.ErrorMessage ?? "Failed to delete chore." });
            }
            return NoContent();
        }

        // POST api/chores/{id}/critical
        [HttpPost("{id}/critical")]
        public async Task<IActionResult> MarkCriticalAsync(string id, [FromBody] CriticalChoreDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Decode chore ID
            if (!IdObfuscator.TryDecode(id, out int choreId))
            {
                return BadRequest(new { error = "Invalid id format." });
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

        // DELETE api/chores/{id}/critical
        [HttpDelete("{id}/critical")]
        public async Task<IActionResult> RemoveCriticalAsync(string id, CancellationToken cancellationToken)
        {
            // Decode chore ID
            if (!IdObfuscator.TryDecode(id, out int choreId))
            {
                return BadRequest(new { error = "Invalid id format." });
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
