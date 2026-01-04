using Bureau.AspNetCore.Problems;
using Bureau.AspNetCore.Tracing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bureau.AspNetCore.Mappers
{
    public static class ResultErrorMapperExtension
    {
        public static ProblemDetails ToProblemDetails(this ResultError error, HttpContext httpContext)
        {
            string traceId = TraceIdAccessor.GetTraceId(httpContext);
            ProblemCodeDefinition pcDefinition = ProblemCodeDefinitionResolver.Resolve(error.Code);
            ProblemDetails problemDetails = new ProblemDetails
            {
                Status = pcDefinition.StatusCode,
                Title = pcDefinition.Title,
                Detail = error.ErrorMessage,
                Instance = httpContext.Request.Path,
                Type = ProblemDetailsTypeResolver.Resolve(error.Code)
            };

            problemDetails.Extensions[ProblemDetailsExtensionNames.Code] = error.Code;
            problemDetails.Extensions[ProblemDetailsExtensionNames.TraceId] = traceId;

            return problemDetails;
        }
        public static ProblemDetails ToProblemDetails(this ResultError error, HttpContext httpContext, int statusCode)
        {
            ProblemDetails problemDetails = error.ToProblemDetails(httpContext);
            problemDetails.Status = statusCode;
            return problemDetails;
        }
    }
}
