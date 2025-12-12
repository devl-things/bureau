using Bureau;
using Microsoft.AspNetCore.Mvc;
using Niles.Chores.Api.Dtos;
using Niles.Chores.Api.Factories;
using Niles.Chores.Api.Mappers;
using Niles.Chores.Api.Utilities;
using Niles.Chores.Services;

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

            return Ok(pagedResult.ToPagedResponse(x => x.ToDto(IdObfuscator.Encode)));
        }

        // GET api/housekeeping/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(string id, CancellationToken cancellationToken)
        {
            if (!IdObfuscator.TryDecode(id, out int intId))
            {
                return BadRequest(new { error = ErrorMessages.InvalidIdFormat });
            }

            Result<Housekeeping> result = await _housekeepingService.GetHousekeepingAsync(intId, cancellationToken);
            if (result.IsError)
            {
                return NotFound();
            }
            HousekeepingDto dto = result.Value.ToDto(IdObfuscator.Encode);
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
            List<int> completedChoreIds = [];
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
            if (result.IsError)
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

            Result<Housekeeping> modelResult = dto.ToResultModel();
            if (modelResult.IsError)
            {
                return BadRequest(new { error = modelResult.Error.ErrorMessage });
            }

            Result<Housekeeping> result = await _housekeepingService.CreateHousekeepingAsync(modelResult.Value, cancellationToken);
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

            if (!IdObfuscator.TryDecode(id, out int intId))
            {
                return BadRequest(new { error = ErrorMessages.InvalidIdFormat });
            }

            Result<Housekeeping> modelResult = dto.ToResultModel();
            if (modelResult.IsError)
            {
                return BadRequest(new { error = modelResult.Error.ErrorMessage });
            }

            modelResult.Value.Id = intId;

            Result<Housekeeping> result = await _housekeepingService.UpdateHousekeepingAsync(modelResult.Value, cancellationToken);
            if (result.IsError)
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
                return BadRequest(new { error = ErrorMessages.InvalidIdFormat });
            }

            Result result = await _housekeepingService.DeleteHousekeepingAsync(intId, cancellationToken);
            if (result.IsError)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
