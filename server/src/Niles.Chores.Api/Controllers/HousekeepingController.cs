using Microsoft.AspNetCore.Mvc;
using Niles.Chores.Api.Dtos;
using Niles.Chores;
using Niles.Chores.Api.Utilities;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Niles.Chores.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HousekeepingController : ControllerBase
    {
        private readonly IHouseKeepingService _housekeepingService;

        public HousekeepingController(IHouseKeepingService housekeepingService)
        {
            _housekeepingService = housekeepingService;
        }

        // GET: api/housekeeping?search=term&page=1&pageSize=10
        // GET: api/housekeeping (returns all housekeeping records)
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            // Validate pagination parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            // Get all housekeeping records
            IEnumerable<Housekeeping> allItems = await _housekeepingService.ListHousekeepingsAsync(cancellationToken);

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLowerInvariant();
                allItems = allItems.Where(h =>
                    (h.Note?.ToLowerInvariant().Contains(searchLower) ?? false) ||
                    h.DateTime.ToString("yyyy-MM-dd").Contains(searchLower) ||
                    h.CompletedChoreIds.Any(id => id.ToString().Contains(searchLower))
                );
            }

            // Order by date descending (most recent first)
            allItems = allItems.OrderByDescending(h => h.DateTime);

            var itemsList = allItems.ToList();
            var total = itemsList.Count;
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            // Apply pagination
            var pagedItems = itemsList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new HousekeepingDto
                {
                    Id = m.Id.HasValue ? IdObfuscator.Encode(m.Id.Value) : null,
                    DateTime = m.DateTime.UtcDateTime,
                    Duration = m.Duration.ToString(),
                    Note = m.Note,
                    CompletedChoreIds = m.CompletedChoreIds ?? new List<int>()
                });

            var result = new PagedResult<HousekeepingDto>
            {
                Data = pagedItems,
                Meta = new PagedMeta
                {
                    Page = page,
                    PageSize = pageSize,
                    Total = total,
                    TotalPages = totalPages,
                    HasNext = page < totalPages,
                    HasPrevious = page > 1
                }
            };

            return Ok(result);
        }

        // GET api/housekeeping/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id, CancellationToken cancellationToken)
        {
            if (!IdObfuscator.TryDecode(id, out int intId))
            {
                return BadRequest(new { error = "Invalid id format." });
            }

            Housekeeping? result = await _housekeepingService.GetHousekeepingAsync(intId, cancellationToken);
            if (result == null)
            {
                return NotFound();
            }
            var dto = new HousekeepingDto
            {
                Id = result.Id.HasValue ? IdObfuscator.Encode(result.Id.Value) : null,
                DateTime = result.DateTime.UtcDateTime,
                Duration = result.Duration.ToString(),
                Note = result.Note,
                CompletedChoreIds = result.CompletedChoreIds ?? new List<int>()
            };
            return Ok(dto);
        }

        // POST api/housekeeping
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] HousekeepingDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!TryMapDtoToModel(dto, out Housekeeping model, out string? error))
            {
                return BadRequest(new { error });
            }

            bool created = await _housekeepingService.CreateHousekeepingAsync(model, cancellationToken);
            if (!created)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Failed to create housekeeping." });
            }

            string encodedId = model.Id.HasValue ? IdObfuscator.Encode(model.Id.Value) : string.Empty;
            return CreatedAtAction(nameof(Get), new { id = encodedId }, dto);
        }

        // PUT api/housekeeping/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] HousekeepingDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (dto.Id != null && dto.Id != id)
            {
                return BadRequest(new { error = "Id in body does not match route id." });
            }

            if (!TryMapDtoToModel(dto, out Housekeeping model, out string? error))
            {
                return BadRequest(new { error });
            }

            if (!IdObfuscator.TryDecode(id, out int intId))
            {
                return BadRequest(new { error = "Invalid id format." });
            }

            model.Id = intId;

            bool updated = await _housekeepingService.UpdateHousekeepingAsync(model, cancellationToken);
            if (!updated)
            {
                return NotFound();
            }
            return NoContent();
        }

        // DELETE api/housekeeping/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            if (!IdObfuscator.TryDecode(id, out int intId))
            {
                return BadRequest(new { error = "Invalid id format." });
            }

            bool deleted = await _housekeepingService.DeleteHousekeepingAsync(intId, cancellationToken);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }

        private static bool TryMapDtoToModel(HousekeepingDto dto, out Housekeeping model, out string? error)
        {
            model = new Housekeeping();
            error = null;

            // map DateTime
            try
            {
                model.DateTime = new DateTimeOffset(dto.DateTime);
            }
            catch (Exception ex)
            {
                error = $"Invalid datetime: {ex.Message}";
                return false;
            }

            // parse duration
            if (!TimeSpan.TryParse(dto.Duration, out TimeSpan duration))
            {
                error = "Invalid duration format. Expected a TimeSpan parsable string like 'hh:mm:ss'.";
                return false;
            }
            model.Duration = duration;

            model.Note = dto.Note;
            model.CompletedChoreIds = dto.CompletedChoreIds ?? new List<int>();
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
                model.Id = null;
            }

            return true;
        }
    }
}
