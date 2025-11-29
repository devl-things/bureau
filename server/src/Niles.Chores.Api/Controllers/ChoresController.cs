using Microsoft.AspNetCore.Mvc;
using Niles.Chores;
using Niles.Chores.Abstractions.Services;
using Niles.Chores.Api.Dtos;
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
            [FromQuery] string? date,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            // If date parameter is provided, return prioritized chores for that date (no pagination for prioritized)
            if (!string.IsNullOrEmpty(date))
            {
                if (DateOnly.TryParse(date, out DateOnly dateOnly))
                {
                    var prioritizedChores = await _prioritizedChoreService.GetPrioritizedChoresAsync(dateOnly, cancellationToken);
                    var prioritizedChoreIds = prioritizedChores.Select(c => c.Id).ToList();
                    var criticalPrioritizedIds = await _criticalChoreService.GetOpenCriticalChoreIdsAsync(prioritizedChoreIds, cancellationToken);
                    var criticalSet = new HashSet<int>(criticalPrioritizedIds);
                    
                    var dtos = prioritizedChores.Select(c => new ChoreDto
                    {
                        Id = IdObfuscator.Encode(c.Id),
                        Title = c.Title,
                        Description = c.Description ?? string.Empty,
                        Priority = c.Priority,
                        Type = c.Type.ToString(),
                        WeeklyInterval = c.WeeklyInterval,
                        Completed = false, // Prioritized chores are not completed by default
                        IsCritical = criticalSet.Contains(c.Id)
                    });
                    return Ok(dtos);
                }
                else
                {
                    return BadRequest(new { error = "Invalid date format. Expected YYYY-MM-DD." });
                }
            }

            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            // Get paged chores from database (with search and pagination at DB level)
            var pagedResult = await _choreService.ListChoresPagedAsync(search, page, pageSize, cancellationToken);
            
            // Get all chore IDs for this page
            var choreIds = pagedResult.Items.Select(c => c.Id).ToList();
            
            // Get all open critical chores for these chore IDs in one query
            var openCriticalChoreIds = await _criticalChoreService.GetOpenCriticalChoreIdsAsync(choreIds, cancellationToken);
            var criticalChoreIds = new HashSet<int>(openCriticalChoreIds);

            // Map to DTOs with critical flag
            var pagedItems = pagedResult.Items.Select(m => new ChoreDto
            {
                Id = IdObfuscator.Encode(m.Id),
                Title = m.Title,
                Description = m.Description,
                Type = m.Type.ToString(),
                WeeklyInterval = m.WeeklyInterval,
                IsCritical = criticalChoreIds.Contains(m.Id)
            });

            var result = new PagedResult<ChoreDto>
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

            Chore? result = await _choreService.GetChoreAsync(intId, cancellationToken);
            if (result == null)
            {
                return NotFound();
            }
            var isCritical = await _criticalChoreService.HasOpenCriticalChoreAsync(intId, cancellationToken);
            var dto = new ChoreDto
            {
                Id = IdObfuscator.Encode(result.Id),
                Title = result.Title,
                Description = result.Description,
                Type = result.Type.ToString(),
                WeeklyInterval = result.WeeklyInterval,
                IsCritical = isCritical
            };
            return Ok(dto);
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

            bool created = await _choreService.CreateChoreAsync(model, cancellationToken);
            if (!created)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to create chore." });
            }

            string encodedId = IdObfuscator.Encode(model.Id);
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

            bool updated = await _choreService.UpdateChoreAsync(model, cancellationToken);
            if (!updated)
            {
                return NotFound();
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

            bool deleted = await _choreService.DeleteChoreAsync(intId, cancellationToken);
            if (!deleted)
            {
                return NotFound();
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

            // Verify chore exists
            var chore = await _choreService.GetChoreAsync(choreId, cancellationToken);
            if (chore == null)
            {
                return NotFound(new { error = "Chore not found." });
            }

            // Check if already has an open critical chore
            var hasOpen = await _criticalChoreService.HasOpenCriticalChoreAsync(choreId, cancellationToken);
            if (hasOpen)
            {
                return BadRequest(new { error = "This chore already has an open critical status. Please remove it first." });
            }

            // Create critical chore
            bool created = await _criticalChoreService.CreateCriticalChoreAsync(choreId, dto.Description, cancellationToken);
            if (!created)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to mark chore as critical." });
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

            // Verify chore exists
            var chore = await _choreService.GetChoreAsync(choreId, cancellationToken);
            if (chore == null)
            {
                return NotFound(new { error = "Chore not found." });
            }

            // Delete critical chore
            bool deleted = await _criticalChoreService.DeleteCriticalChoreAsync(choreId, cancellationToken);
            if (!deleted)
            {
                return BadRequest(new { error = "No open critical status found for this chore." });
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
