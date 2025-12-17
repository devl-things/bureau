using Bureau.AspNetCore.Logging;
using Bureau.AspNetCore.Mappers;
using Bureau.AspNetCore.Problems;
using Bureau.Server.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Bureau.AspNetCore.Controllers
{
    public class BureauApiControllerBase : ControllerBase
    {
        protected readonly ILogger _logger;

        public BureauApiControllerBase(ILogger logger)
        {
            _logger = logger;
        }

        protected IActionResult ProblemDetailsResponse(ModelStateDictionary modelState)
        {
            return StatusCode(StatusCodes.Status400BadRequest, ValidationProblemDetailsFactory.Create(HttpContext, modelState));
        }
        protected IActionResult ProblemDetailsResponse(ResultError error)
        {
            _logger.LogResultError(error, HttpContext);
            ProblemDetails problem = error.ToProblemDetails(HttpContext);
            int statusCode = problem.Status ?? StatusCodes.Status500InternalServerError;
            return StatusCode(statusCode, problem);
        }
        protected IActionResult ProblemDetailsResponse(ResultError error, int statusCode)
        {
            _logger.LogResultError(error, HttpContext);
            return StatusCode(statusCode, error.ToProblemDetails(HttpContext, statusCode));
        }
        protected IActionResult OkResponse<T>(T value)
        {
            return Ok(new BureauResponse<T>() { Data = value });
        }
    }
}
