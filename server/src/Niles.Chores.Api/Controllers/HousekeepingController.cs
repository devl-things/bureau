using Bureau;
using Bureau.AspNetCore.Controllers;
using Bureau.Primitives.Errors;
using Bureau.Server.Contracts;
using Bureau.Server.Contracts.Mappers;
using Microsoft.AspNetCore.Mvc;
using Niles.Chores.Api.Dtos;
using Niles.Chores.Api.Factories;
using Niles.Chores.Api.Mappers;
using Niles.Chores.Services;

namespace Niles.Chores.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HousekeepingController : BureauApiControllerBase
    {
        private readonly IHouseKeepingService _housekeepingService;
        private readonly IIdObfuscator _idObfuscator;

        public HousekeepingController(ILogger<HousekeepingController> logger,
            IHouseKeepingService housekeepingService, IIdObfuscator idObfuscator) : base(logger)
        {
            _housekeepingService = housekeepingService;
            _idObfuscator = idObfuscator;
        }

        // GET: api/housekeeping?search=term&page=1&pageSize=10
        // GET: api/housekeeping (returns first 20 records)
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] SearchQueryDto queryParams, CancellationToken cancellationToken = default)
        {
            SearchParameters searchParameters = SearchParametersFactory.Create(queryParams);

            PagedResult<Housekeeping> pagedResult = await _housekeepingService.GetHousekeepingsAsync(searchParameters, cancellationToken);

            return Ok(pagedResult.ToPagedResponse(x => x.ToDto(_idObfuscator)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            Result<int> idResult = _idObfuscator.Decode(id);
            if (idResult.IsError)
            {
                return ProblemDetailsResponse(idResult.Error);
            }

            Result<Housekeeping> result = await _housekeepingService.GetHousekeepingAsync(idResult.Value, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }
            return OkResponse(result.Value.ToDto(_idObfuscator));
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitAsync([FromBody] CreateHousekeepingRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ProblemDetailsResponse(ModelState);
            }
            await CreateHousekeepingAsync(dto, cancellationToken);
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] CreateHousekeepingRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ProblemDetailsResponse(ModelState);
            }
            Result<Housekeeping> result = await CreateHousekeepingAsync(dto, cancellationToken);

            return CreatedAtAction(nameof(GetByIdAsync), new { id = _idObfuscator.Encode(result.Value.Id) }, new BureauResponse<HousekeepingDto>(result.Value.ToDto(_idObfuscator)));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(string id, [FromBody] UpdateHousekeepingRequest dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ProblemDetailsResponse(ModelState);
            }

            if (string.IsNullOrEmpty(dto.Id) || dto.Id != id)
            {
                return ProblemDetailsResponse(ResultError.From(ProblemCodes.Request.IdMismatch, "Id in body does not match route id."));
            }

            Result<Housekeeping> housekeepingResult = dto.ToResultModel(_idObfuscator);
            if (housekeepingResult.IsError)
            {
                return ProblemDetailsResponse(housekeepingResult.Error);
            }

            Result<Housekeeping> result = await _housekeepingService.UpdateHousekeepingAsync(housekeepingResult.Value, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            Result<int> idResult = _idObfuscator.Decode(id);
            if (idResult.IsError)
            {
                return ProblemDetailsResponse(idResult.Error);
            }

            Result result = await _housekeepingService.DeleteHousekeepingAsync(idResult.Value, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }
            return NoContent();
        }

        private async Task<Result<Housekeeping>> CreateHousekeepingAsync(CreateHousekeepingRequest dto, CancellationToken cancellationToken)
        {
            Result<Housekeeping> housekeepingResult = dto.ToResultModel(_idObfuscator);
            if (housekeepingResult.IsError)
            {
                return housekeepingResult.Error;
            }

            Result<Housekeeping> result = await _housekeepingService.CreateHousekeepingAsync(housekeepingResult.Value, cancellationToken);
            return result;
        }
    }
}
