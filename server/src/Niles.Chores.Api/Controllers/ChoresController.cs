using Bureau;
using Bureau.AspNetCore.Controllers;
using Bureau.Primitives.Errors;
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
    public class ChoresController : BureauApiControllerBase
    {
        private readonly IChoreService _choreService;

        public ChoresController(ILogger<ChoresController> logger, IChoreService choreService) : base(logger)
        {
            _choreService = choreService;
        }

        // GET: api/chores?search=term&page=1&pageSize=10 (returns paginated chores with search)
        // GET: api/chores (returns first page)
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] SearchQueryDto queryParams, CancellationToken cancellationToken)
        {
            SearchParameters searchParameters = SearchParametersFactory.Create(queryParams);

            PagedResult<Chore> pagedResult = await _choreService.GetChoresAsync(searchParameters, cancellationToken);

            return Ok(pagedResult.ToPagedResponse(x => x.ToDto(IdObfuscator.Encode)));
        }

        // GET api/chores/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(string id, CancellationToken cancellationToken)
        {
            Result<int> idResult = EntityIdParser.ParseEntityId(id);
            if (idResult.IsError)
            {
                return ProblemDetailsResponse(idResult.Error);
            }

            Result<Chore> result = await _choreService.GetChoreAsync(idResult.Value, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }
            // TODO Bureau.Server.Contracts BureauResponse this also changes on the frontend! so new issue
            return Ok(result.Value.ToDto(IdObfuscator.Encode));
        }

        // POST api/chores
        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] ChoreDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ProblemDetailsResponse(ModelState);
            }

            Result<Chore> modelResult = dto.ToResultModel();
            if (modelResult.IsError)
            {
                return ProblemDetailsResponse(modelResult.Error);
            }

            Result<Chore> result = await _choreService.CreateChoreAsync(modelResult.Value!, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }

            string encodedId = IdObfuscator.Encode(result.Value!.Id);
            dto.Id = encodedId;
            // TODO Bureau.Server.Contracts BureauResponse this also changes on the frontend! so new issue
            return CreatedAtAction(nameof(GetByIdAsync), new { id = encodedId }, dto);
        }

        // PUT api/chores/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAsync(string id, [FromBody] ChoreDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ProblemDetailsResponse(ModelState);
            }

            if (dto.Id != null && dto.Id != id)
            {
                return ProblemDetailsResponse(ResultError.From(ProblemCodes.Request.IdMismatch, "Id in body does not match route id."));
            }
            Result<int> idResult = EntityIdParser.ParseEntityId(id);
            if (idResult.IsError)
            {
                return ProblemDetailsResponse(idResult.Error);
            }

            Result<Chore> modelResult = dto.ToResultModel(idResult.Value);
            if (modelResult.IsError)
            {
                return ProblemDetailsResponse(modelResult.Error);
            }

            Result<Chore> result = await _choreService.UpdateChoreAsync(modelResult.Value!, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }
            return NoContent();
        }

        // DELETE api/chores/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
        {
            Result<int> idResult = EntityIdParser.ParseEntityId(id);
            if (idResult.IsError)
            {
                return ProblemDetailsResponse(idResult.Error);
            }

            Result result = await _choreService.DeleteChoreAsync(idResult.Value, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }
            return NoContent();
        }
    }
}
