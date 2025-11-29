using Microsoft.AspNetCore.Mvc;
using Niles.Chores.Api.Dtos;
using Niles.Chores;
using Niles.Chores.Abstractions.Services;
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

            // Get paged housekeeping records from database (with search and pagination at DB level)
            var pagedResult = await _housekeepingService.ListHousekeepingsPagedAsync(search, page, pageSize, cancellationToken);

            // Map to DTOs
            var pagedItems = pagedResult.Items.Select(m => new HousekeepingDto
            {
                Id = m.Id.HasValue ? IdObfuscator.Encode(m.Id.Value) : null,
                DateTime = m.DateTime.UtcDateTime,
                Duration = m.Duration.ToString(),
                Note = m.Note,
                CompletedChoreIds = m.CompletedChoreIds ?? new List<int>(),
                CompletedChores = m.CompletedChores?.Select(c => new ChoreDto
                {
                    Id = IdObfuscator.Encode(c.Id),
                    Title = c.Title,
                    Description = c.Description ?? string.Empty,
                    Type = c.Type.ToString(),
                    WeeklyInterval = c.WeeklyInterval,
                    Priority = 0,
                    Completed = false,
                    IsCritical = false
                }).ToList() ?? new List<ChoreDto>()
            });

            var result = new PagedResult<HousekeepingDto>
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
                CompletedChoreIds = result.CompletedChoreIds ?? new List<int>(),
                CompletedChores = result.CompletedChores?.Select(c => new ChoreDto
                {
                    Id = IdObfuscator.Encode(c.Id),
                    Title = c.Title,
                    Description = c.Description ?? string.Empty,
                    Type = c.Type.ToString(),
                    WeeklyInterval = c.WeeklyInterval,
                    Priority = 0,
                    Completed = false,
                    IsCritical = false
                }).ToList() ?? new List<ChoreDto>()
            };
            return Ok(dto);
        }

        // POST api/housekeeping/submit
        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] HousekeepingSubmitDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Parse date
            if (!DateOnly.TryParse(dto.Date, out DateOnly dateOnly))
            {
                return BadRequest(new { error = "Invalid date format. Expected YYYY-MM-DD." });
            }

            // Decode completed chore IDs
            var completedChoreIds = new List<int>();
            foreach (var id in dto.CompletedChoreIds)
            {
                if (IdObfuscator.TryDecode(id, out int intId))
                {
                    completedChoreIds.Add(intId);
                }
                else
                {
                    return BadRequest(new { error = $"Invalid chore id format: {id}" });
                }
            }

            // Create housekeeping record
            var housekeeping = new Housekeeping
            {
                DateTime = new DateTimeOffset(dateOnly.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero),
                Duration = dto.Duration.HasValue ? TimeSpan.FromMinutes(dto.Duration.Value) : TimeSpan.Zero,
                Note = dto.Note,
                CompletedChoreIds = completedChoreIds
            };

            var result = await _housekeepingService.CreateHousekeepingAsync(housekeeping, cancellationToken);
            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.ErrorMessage ?? "Failed to submit chores." });
            }

            // Note: Critical chores are automatically marked as completed in HouseKeepingService.CreateHousekeepingAsync

            return Ok(new { message = "Chores submitted successfully", id = result.Value!.Id.HasValue ? IdObfuscator.Encode(result.Value.Id.Value) : null });
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

            var result = await _housekeepingService.CreateHousekeepingAsync(model, cancellationToken);
            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.ErrorMessage ?? "Failed to create housekeeping." });
            }

            string encodedId = result.Value!.Id.HasValue ? IdObfuscator.Encode(result.Value.Id.Value) : string.Empty;
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

            var result = await _housekeepingService.UpdateHousekeepingAsync(model, cancellationToken);
            if (!result.IsSuccess)
            {
                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound();
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.ErrorMessage ?? "Failed to update housekeeping." });
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

            var result = await _housekeepingService.DeleteHousekeepingAsync(intId, cancellationToken);
            if (!result.IsSuccess)
            {
                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound();
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.ErrorMessage ?? "Failed to delete housekeeping." });
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
