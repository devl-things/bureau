using Bureau;
using Microsoft.AspNetCore.Mvc;
using Niles.Chores.Api.Dtos;
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
        private readonly IPrioritizedChoreService _prioritizedChoreService;
        private readonly ICriticalChoreService _criticalChoreService;

        public ChoresController(IChoreService choreService, IPrioritizedChoreService prioritizedChoreService, IHouseKeepingService housekeepingService, ICriticalChoreService criticalChoreService)
        {
            _choreService = choreService;
            _prioritizedChoreService = prioritizedChoreService;
            _criticalChoreService = criticalChoreService;
        }

        // TODO REFACTOR remove api/chores?date=YYYY-MM-DD (returns prioritized chores for date) for that you have a endpoint in HouseKeepingController
        // TODO REFACTOR remove date parameter from this controller and use SearchRequest only for pagination and search
        // GET: api/chores?date=YYYY-MM-DD (returns prioritized chores for date)
        // GET: api/chores?search=term&page=1&pageSize=10 (returns paginated chores with search)
        // GET: api/chores (returns all chores)
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] ChoresQueryParams queryParams, CancellationToken cancellationToken)
        {
            // If date parameter is provided, return prioritized chores for that date (no pagination for prioritized)
            if (!string.IsNullOrEmpty(queryParams.Date))
            {
                if (DateOnly.TryParse(queryParams.Date, out DateOnly dateOnly))
                {
                    IEnumerable<PrioritizedChore> prioritizedChores = await _prioritizedChoreService.GetPrioritizedChoresAsync(dateOnly, cancellationToken);
                    List<int> prioritizedChoreIds = prioritizedChores.Select(c => c.Id).ToList();
                    List<int> criticalPrioritizedIds = await _criticalChoreService.GetOpenCriticalChoreIdsAsync(prioritizedChoreIds, cancellationToken);
                    HashSet<int> criticalSet = new HashSet<int>(criticalPrioritizedIds);

                    // TODO REFACTOR set IsCritical in ToDto mapper
                    IEnumerable<ChoreDto> dtos = prioritizedChores.Select(c => c.ToDto(
                        priority: c.Priority,
                        completed: false
                    ));
                    return Ok(dtos);
                }
                else
                {
                    return BadRequest(new { error = "Invalid date format. Expected YYYY-MM-DD." });
                }
            }

            // Validate pagination parameters
            int page = queryParams.Page < 1 ? 1 : queryParams.Page;
            int pageSize = queryParams.PageSize < 1 ? 20 : queryParams.PageSize;
            if (pageSize > 100) pageSize = 100;

            SearchParameters pagination = new SearchParameters(queryParams.Search, page, pageSize);

            // Get paged chores from database (with search and pagination at DB level)
            PagedResult<Chore> pagedResult = await _choreService.ListChoresPagedWithCriticalAsync(pagination, cancellationToken);

            // Get all chore IDs for this page
            List<int> choreIds = pagedResult.Values.Select(c => c.Id).ToList();

            // Get all open critical chores for these chore IDs in one query
            List<int> openCriticalChoreIds = await _criticalChoreService.GetOpenCriticalChoreIdsAsync(choreIds, cancellationToken);
            HashSet<int> criticalChoreIds = new HashSet<int>(openCriticalChoreIds);

            // Map to DTOs with critical flag
            // TODO REFACTOR set IsCritical in ToDto mapper
            IEnumerable<ChoreDto> pagedItems = pagedResult.Values.Select(m => m.ToDto());
            Dtos.PagedResponse<ChoreDto> result = new Dtos.PagedResponse<ChoreDto>
            {
                Data = pagedItems,
                Meta = new PagedMeta
                {
                    Page = pagedResult.Page,
                    PageSize = pagedResult.PageSize,
                    Total = pagedResult.Count,
                    TotalPages = pagedResult.TotalPages,
                    HasNext = pagedResult.HasNext,
                    HasPrevious = pagedResult.HasPrevious
                }
            };

            return Ok(result);
        }

        // GET api/chores/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            if (!IdObfuscator.TryDecode(id, out int intId))
            {
                return BadRequest(new { error = "Invalid id format." });
            }
            Chore? chore = await _choreService.GetChoreAsync(intId, cancellationToken);
            if (chore == null)
            {
                return NotFound();
            }
            return Ok(chore.ToDto());
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
