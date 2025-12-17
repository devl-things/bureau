using Bureau.Primitives.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bureau.AspNetCore.Problems
{
    public static class ProblemDetailsFactory
    {
        public static ProblemDetails Create(HttpContext context, string traceId)
        {
            string code = ProblemCodes.System.UnexpectedError;
            ProblemCodeDefinition pcDefinition = ProblemCodeDefinitionResolver.Resolve(code);
            ProblemDetails problemDetails = new ProblemDetails
            {
                Status = pcDefinition.StatusCode,
                Title = pcDefinition.Title,
                Detail = null,
                Type = ProblemDetailsTypeResolver.Resolve(code),
                Instance = context.Request.Path
            };

            problemDetails.Extensions[ProblemDetailsExtensionNames.Code] = code;
            problemDetails.Extensions[ProblemDetailsExtensionNames.TraceId] = traceId;

            return problemDetails;
        }
    }
}
