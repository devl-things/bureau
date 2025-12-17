using Bureau;
using Bureau.AspNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Niles.Chores.Api.Dtos;
using Niles.Chores.Api.Utilities;
using Niles.Chores.Services;

namespace Niles.Chores.Api.Controllers
{
    [Route("api/chores/{id}/critical")]
    [ApiController]
    public class CriticalChoresController : BureauApiControllerBase
    {

        private readonly ICriticalChoreService _criticalChoreService;

        public CriticalChoresController(ILogger<CriticalChoresController> logger, ICriticalChoreService criticalChoreService) : base(logger)
        {
            _criticalChoreService = criticalChoreService;
        }

        [HttpPost]
        public async Task<IActionResult> MarkCriticalAsync(string id, [FromBody] CriticalChoreDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ProblemDetailsResponse(ModelState);
            }
            Result<int> idResult = EntityIdParser.ParseEntityId(id);
            if (idResult.IsError)
            {
                return ProblemDetailsResponse(idResult.Error);
            }

            Result result = await _criticalChoreService.CreateCriticalChoreAsync(idResult.Value, dto.Description, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> RemoveCriticalAsync(string id, CancellationToken cancellationToken)
        {
            Result<int> idResult = EntityIdParser.ParseEntityId(id);
            if (idResult.IsError)
            {
                return ProblemDetailsResponse(idResult.Error);
            }
            Result result = await _criticalChoreService.DeleteCriticalChoreAsync(idResult.Value, cancellationToken);
            if (result.IsError)
            {
                return ProblemDetailsResponse(result.Error);
            }
            return NoContent();
        }
    }
}
