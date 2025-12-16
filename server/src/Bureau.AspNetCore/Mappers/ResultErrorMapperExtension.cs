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
                Title = GetDefaultTitle(statusCode),
                Detail = error.ErrorMessage,
                Instance = httpContext.Request.Path,
                Type = ProblemDetailsType.FromCode(error.Code)
            };

            problemDetails.Extensions[ProblemDetailsExtensionNames.Code] = error.Code;
            problemDetails.Extensions[ProblemDetailsExtensionNames.TraceId] = traceId;

            return problemDetails;
        }

        private static string GetDefaultTitle(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "Request is invalid.",
                StatusCodes.Status401Unauthorized => "Unauthorized.",
                StatusCodes.Status403Forbidden => "Forbidden.",
                StatusCodes.Status404NotFound => "Not found.",
                StatusCodes.Status409Conflict => "Conflict.",
                _ => "Request failed."
            };
        }
    }
}
