using Microsoft.AspNetCore.Mvc;
using Niles.Chores;
using Niles.Chores.Abstractions.Models;
using Niles.Chores.Abstractions.Services;
using Niles.Chores.Api.Dtos;
using Niles.Chores.Api.Mappers;
using Niles.Chores.Api.Utilities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Niles.Chores.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChoresController : ControllerBase
    {
        private readonly IChoreService _choreService;
        private readonly IPrioritizedChoreService _prioritizedChoreService;
        private readonly IHouseKeepingService _housekeepingService;
        private readonly ICriticalChoreService _criticalChoreService;

        public ChoresController(IChoreService choreService, IPrioritizedChoreService prioritizedChoreService, IHouseKeepingService housekeepingService, ICriticalChoreService criticalChoreService)
        {
            _choreService = choreService;
            _prioritizedChoreService = prioritizedChoreService;
            _housekeepingService = housekeepingService;
            _criticalChoreService = criticalChoreService;
        }

        // GET: api/chores?date=YYYY-MM-DD (returns prioritized chores for date)
        // GET: api/chores?search=term&page=1&pageSize=10 (returns paginated chores with search)
        // GET: api/chores (returns all chores)
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] ChoresQueryParams queryParams,
            CancellationToken cancellationToken = default)
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
                    
                    IEnumerable<ChoreDto> dtos = prioritizedChores.Select(c => c.ToDto(
                        isCritical: criticalSet.Contains(c.Id),
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

            PaginationParams pagination = new PaginationParams(queryParams.Search, page, pageSize);

            // Get paged chores from database (with search and pagination at DB level)
            Niles.Chores.Abstractions.Models.PagedResult<Chore> pagedResult = await _choreService.ListChoresPagedWithCriticalAsync(pagination, cancellationToken);
            
            // Get all chore IDs for this page
            List<int> choreIds = pagedResult.Items.Select(c => c.Id).ToList();
            
            // Get all open critical chores for these chore IDs in one query
            List<int> openCriticalChoreIds = await _criticalChoreService.GetOpenCriticalChoreIdsAsync(choreIds, cancellationToken);
            HashSet<int> criticalChoreIds = new HashSet<int>(openCriticalChoreIds);

            // Map to DTOs with critical flag
            IEnumerable<ChoreDto> pagedItems = pagedResult.Items.Select(m => m.ToDto(isCritical: criticalChoreIds.Contains(m.Id)));
            Dtos.PagedResult<ChoreDto> result = new Dtos.PagedResult<ChoreDto>
            {
                Data = pagedItems,
                Meta = new PagedMeta
                {
                    Page = pagedResult.Page,
                    PageSize = pagedResult.PageSize,
                    Total = pagedResult.Total,
                    TotalPages = pagedResult.TotalPages,
                    HasNext = pagedResult.HasNext,
                    HasPrevious = pagedResult.HasPrevious
                }
            };

            return Ok(result);
        }

        // GET api/chores/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id, CancellationToken cancellationToken)
        {
            if (!IdObfuscator.TryDecode(id, out int intId))
            {
                return BadRequest(new { error = "Invalid id format." });
            }

            (Chore Chore, bool IsCritical)? result = await _choreService.GetChoreWithCriticalAsync(intId, cancellationToken);
            if (result == null)
            {
                return NotFound();
            }
            (Chore chore, bool isCritical) = result.Value;
            return Ok(chore.ToDto(isCritical: isCritical));
        }

        // POST api/chores
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChoreDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!TryMapDtoToModel(dto, out Chore model, out string? error))
            {
                return BadRequest(new { error });
            }

            Result<Chore> result = await _choreService.CreateChoreAsync(model, cancellationToken);
            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.ErrorMessage ?? "Failed to create chore." });
            }

            string encodedId = IdObfuscator.Encode(result.Value!.Id);
            dto.Id = encodedId;
            return CreatedAtAction(nameof(Get), new { id = encodedId }, dto);
        }

        // PUT api/chores/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] ChoreDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.Id != null && dto.Id != id)
            {
                return BadRequest(new { error = "Id in body does not match route id." });
            }

            if (!TryMapDtoToModel(dto, out Chore model, out string? error))
            {
                return BadRequest(new { error });
            }

            if (!IdObfuscator.TryDecode(id, out int intId))
            {
                return BadRequest(new { error = "Invalid id format." });
            }

            model.Id = intId;

            Result<Chore> result = await _choreService.UpdateChoreAsync(model, cancellationToken);
            if (!result.IsSuccess)
            {
                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound();
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.ErrorMessage ?? "Failed to update chore." });
            }
            return NoContent();
        }

        // DELETE api/chores/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            if (!IdObfuscator.TryDecode(id, out int intId))
            {
                return BadRequest(new { error = "Invalid id format." });
            }

            Result result = await _choreService.DeleteChoreAsync(intId, cancellationToken);
            if (!result.IsSuccess)
            {
                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound();
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.ErrorMessage ?? "Failed to delete chore." });
            }
            return NoContent();
        }

        // POST api/chores/{id}/critical
        [HttpPost("{id}/critical")]
        public async Task<IActionResult> MarkCritical(string id, [FromBody] CriticalChoreDto dto, CancellationToken cancellationToken)
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
                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(new { error = result.ErrorMessage });
                }
                if (result.ErrorMessage?.Contains("already has an open critical status") == true)
                {
                    return BadRequest(new { error = result.ErrorMessage });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.ErrorMessage ?? "Failed to mark chore as critical." });
            }

            return Ok(new { message = "Chore marked as critical successfully" });
        }

        // DELETE api/chores/{id}/critical
        [HttpDelete("{id}/critical")]
        public async Task<IActionResult> RemoveCritical(string id, CancellationToken cancellationToken)
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
                if (result.ErrorMessage?.Contains("No open critical status") == true)
                {
                    return BadRequest(new { error = result.ErrorMessage });
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.ErrorMessage ?? "Failed to remove critical status." });
            }

            return Ok(new { message = "Critical status removed successfully" });
        }

        private static bool TryMapDtoToModel(ChoreDto dto, out Chore model, out string? error)
        {
            model = new Chore();
            error = null;

            // parse type
            if (!Enum.TryParse<ChoreType>(dto.Type, ignoreCase: true, out ChoreType type))
            {
                error = $"Invalid type. Expected one of: {string.Join(", ", Enum.GetNames<ChoreType>())}.";
                return false;
            }
            model.Type = type;

            model.Title = dto.Title;
            model.Description = dto.Description;
            model.WeeklyInterval = dto.WeeklyInterval;

            if (!string.IsNullOrEmpty(dto.Id))
            {
                if (!IdObfuscator.TryDecode(dto.Id!, out int parsed))
                {
                    error = "Invalid id format in body.";
                    return false;
                }
                model.Id = parsed;
            }
            else
            {
                model.Id = 0;
            }

            return true;
        }
    }
}
