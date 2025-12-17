using Bureau.AspNetCore.Problems;
using Bureau.AspNetCore.Tracing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Bureau.AspNetCore.Mappers
{
    public static class ResultErrorMapperExtension
    {
        public static ProblemDetails ToProblemDetails(this ResultError error, HttpContext httpContext, int statusCode)
        {
            string traceId = TraceIdAccessor.GetTraceId(httpContext);

            ProblemDetails problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = ProblemDetailsTitleResolver.Resolve(error.Code),
                Detail = error.ErrorMessage,
                Instance = httpContext.Request.Path,
                Type = ProblemDetailsTypeResolver.Resolve(error.Code)
            };

            problemDetails.Extensions[ProblemDetailsExtensionNames.Code] = error.Code;
            problemDetails.Extensions[ProblemDetailsExtensionNames.TraceId] = traceId;

            return problemDetails;
        }
    }
}
