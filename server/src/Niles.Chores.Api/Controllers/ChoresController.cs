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
    public class ChoresController : ControllerBase
    {
        private readonly IChoreService _choreService;

        public ChoresController(IChoreService choreService)
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
            if (!IdObfuscator.TryDecode(id, out int intId))
            {
                return BadRequest(new { error = ErrorMessages.InvalidIdFormat });
            }
            Result<Chore> result = await _choreService.GetChoreAsync(intId, cancellationToken);
            if (result.IsError)
            {
                return NotFound();
            }
            return Ok(result.Value.ToDto(IdObfuscator.Encode));
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
            if (resultModel.IsError)
            {
                return BadRequest(new { error = resultModel.Error.ErrorMessage });
            }

            Result<Chore> result = await _choreService.CreateChoreAsync(resultModel.Value!, cancellationToken);
            if (result.IsError)
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
                return BadRequest(new { error = ErrorMessages.InvalidIdFormat });
            }

            Result<Chore> modelResult = dto.ToResultModel(intId);

            if (modelResult.IsError)
            {
                return BadRequest(new { error = modelResult.Error.ErrorMessage });
            }

            Result<Chore> result = await _choreService.UpdateChoreAsync(modelResult.Value!, cancellationToken);
            if (result.IsError)
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
                return BadRequest(new { error = ErrorMessages.InvalidIdFormat });
            }

            Result result = await _choreService.DeleteChoreAsync(intId, cancellationToken);
            if (result.IsError)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
