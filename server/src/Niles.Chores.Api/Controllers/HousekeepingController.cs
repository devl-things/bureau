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
        public async Task<IActionResult> GetAsync([FromQuery] SearchQueryDto queryParams, CancellationToken cancellationToken = default)
        {
            SearchParameters searchParameters = SearchParametersFactory.Create(queryParams);

            PagedResult<Housekeeping> pagedResult = await _housekeepingService.GetHousekeepingsAsync(searchParameters, cancellationToken);

            return Ok(pagedResult.ToPagedResponse(x => x.ToDto()));
        }

        // GET api/housekeeping/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(string id, CancellationToken cancellationToken)
        {
            if (!IdObfuscator.TryDecode(id, out int intId))
            {
                return BadRequest(new { error = "Invalid id format." });
            }

            Result<Housekeeping> result = await _housekeepingService.GetHousekeepingAsync(intId, cancellationToken);
            if (result.IsError)
            {
                return NotFound();
            }
            HousekeepingDto dto = result.Value.ToDto();
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

            if (!ChoresDateParser.TryParseDateTime(dto.Date, out DateTimeOffset dateTime))
            {
                return BadRequest(new { error = "Invalid date format." });
            }

            if (!ChoresDateParser.TryParseDuration(dto.Duration, out TimeSpan duration) || duration == TimeSpan.Zero)
            {
                return BadRequest(new { error = "Duration should be set" });
            }

            // Decode completed chore IDs
            List<int> completedChoreIds = new List<int>();
            foreach (string id in dto.CompletedChoreIds)
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

            Housekeeping housekeeping = new Housekeeping
            {
                DateTime = dateTime,
                Duration = duration,
                Note = dto.Note,
                CompletedChoreIds = completedChoreIds
            };

            Result<Housekeeping> result = await _housekeepingService.CreateHousekeepingAsync(housekeeping, cancellationToken);
            if (!result.IsSuccess)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error.ErrorMessage ?? "Failed to submit chores." });
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

            Result<Housekeeping> result = await _housekeepingService.CreateHousekeepingAsync(model, cancellationToken);
            if (result.IsError)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error.ErrorMessage ?? "Failed to create housekeeping." });
            }

            string encodedId = result.Value!.Id.HasValue ? IdObfuscator.Encode(result.Value.Id.Value) : string.Empty;
            return CreatedAtAction(nameof(GetAsync), new { id = encodedId }, dto);
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

            Result<Housekeeping> result = await _housekeepingService.UpdateHousekeepingAsync(model, cancellationToken);
            if (!result.IsSuccess)
            {
                if (result.Error.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound();
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error.ErrorMessage ?? "Failed to update housekeeping." });
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

            Result result = await _housekeepingService.DeleteHousekeepingAsync(intId, cancellationToken);
            if (result.IsError)
            {
                if (result.Error.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound();
                }
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = result.Error.ErrorMessage ?? "Failed to delete housekeeping." });
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
            if (!ChoresDateParser.TryParseDuration(dto.Duration, out TimeSpan duration))
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
