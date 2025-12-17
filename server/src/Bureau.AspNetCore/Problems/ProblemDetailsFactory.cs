using Bureau.Primitives.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bureau.AspNetCore.Problems
{
    public static class ProblemDetailsFactory
    {
        public static ProblemDetails Create(HttpContext context, string traceId)
        {
            ProblemDetails problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = ProblemDetailsTitleResolver.Resolve(StatusCodes.Status500InternalServerError),
                Detail = null,
                Type = ProblemDetailsTypeResolver.Resolve(ProblemCodes.System.UnexpectedError),
                Instance = context.Request.Path
            };

            problemDetails.Extensions[ProblemDetailsExtensionNames.Code] = ProblemCodes.System.UnexpectedError;
            problemDetails.Extensions[ProblemDetailsExtensionNames.TraceId] = traceId;

            return problemDetails;
        }
    }
}
