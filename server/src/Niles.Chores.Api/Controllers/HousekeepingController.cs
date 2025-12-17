using Bureau;
using Bureau.AspNetCore.Controllers;
using Bureau.Server.Contracts.Mappers;
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
    public class HousekeepingController : BureauApiControllerBase
    {
        private readonly IHouseKeepingService _housekeepingService;

        public HousekeepingController(ILogger<HousekeepingController> logger, IHouseKeepingService housekeepingService) : base(logger)
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
        public async Task<IActionResult> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            Result<int> idResult = EntityIdParser.ParseEntityId(id);
            if (idResult.IsError)
            {
                return ProblemDetailsResponse(StatusCodes.Status400BadRequest, idResult.Error);
            }

            Result<Housekeeping> result = await _housekeepingService.GetHousekeepingAsync(idResult.Value, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(StatusCodes.Status404NotFound, result.Error);
            }
            // TODO Bureau.Server.Contracts BureauResponse this also changes on the frontend! so new issue
            HousekeepingDto dto = result.Value.ToDto(IdObfuscator.Encode);
            return Ok(dto);
        }

        // POST api/housekeeping/submit
        [HttpPost("submit")]
        public async Task<IActionResult> SubmitAsync([FromBody] HousekeepingSubmitDto dto, CancellationToken cancellationToken)
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
        public async Task<IActionResult> PostAsync([FromBody] HousekeepingDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ProblemDetailsResponse(ModelState);
            }

            Result<Housekeeping> modelResult = dto.ToResultModel();
            if (modelResult.IsError)
            {
                return ProblemDetailsResponse(StatusCodes.Status400BadRequest, modelResult.Error);
            }

            Result<Housekeeping> result = await _housekeepingService.CreateHousekeepingAsync(modelResult.Value, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(StatusCodes.Status400BadRequest, result.Error);
            }

            string encodedId = result.Value!.Id.HasValue ? IdObfuscator.Encode(result.Value.Id.Value) : string.Empty;
            // TODO Bureau.Server.Contracts BureauResponse this also changes on the frontend! so new issue
            return CreatedAtAction(nameof(GetByIdAsync), new { id = encodedId }, dto);
        }

        // PUT api/housekeeping/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] HousekeepingDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ProblemDetailsResponse(ModelState);
            }

            if (dto.Id != null && dto.Id != id)
            {
                //TODO deal with ResultError creating
                return BadRequest(new { error = "Id in body does not match route id." });
            }

            Result<int> idResult = EntityIdParser.ParseEntityId(id);
            if (idResult.IsError)
            {
                return ProblemDetailsResponse(StatusCodes.Status400BadRequest, idResult.Error);
            }

            Result<Housekeeping> modelResult = dto.ToResultModel();
            if (modelResult.IsError)
            {
                return ProblemDetailsResponse(StatusCodes.Status400BadRequest, modelResult.Error);
            }
            //TODO put this id in ToResultModel like chores
            modelResult.Value.Id = idResult.Value;

            Result<Housekeeping> result = await _housekeepingService.UpdateHousekeepingAsync(modelResult.Value, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(StatusCodes.Status404NotFound, result.Error);
            }
            return NoContent();
        }

        // DELETE api/housekeeping/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            Result<int> idResult = EntityIdParser.ParseEntityId(id);
            if (idResult.IsError)
            {
                return ProblemDetailsResponse(StatusCodes.Status400BadRequest, idResult.Error);
            }

            Result result = await _housekeepingService.DeleteHousekeepingAsync(idResult.Value, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(StatusCodes.Status404NotFound, result.Error);
            }
            return NoContent();
        }
    }
}
