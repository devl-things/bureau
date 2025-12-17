using Bureau.AspNetCore.Tracing;
using Bureau.Primitives.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Bureau.AspNetCore.Problems
{
    public static class ValidationProblemDetailsFactory
    {
        public static ValidationProblemDetails Create(HttpContext httpContext, ModelStateDictionary modelState)
        {
            ValidationProblemDetails problem = new ValidationProblemDetails(modelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = ProblemDetailsTitleResolver.Resolve(StatusCodes.Status400BadRequest),
                Instance = httpContext.Request.Path,
                Type = ProblemDetailsTypeResolver.Resolve(ProblemCodes.Validation.Failed)
            };

            problem.Extensions[ProblemDetailsExtensionNames.Code] = ProblemCodes.Validation.Failed;
            problem.Extensions[ProblemDetailsExtensionNames.TraceId] = TraceIdAccessor.GetTraceId(httpContext);

            return problem;
        }

        public static ValidationProblemDetails Create(HttpContext httpContext, IDictionary<string, string[]> errors)
        {
            ValidationProblemDetails problem = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = ProblemDetailsTitleResolver.Resolve(StatusCodes.Status400BadRequest),
                Instance = httpContext.Request.Path,
                Type = ProblemDetailsTypeResolver.Resolve(ProblemCodes.Validation.Failed)
            };

            problem.Extensions[ProblemDetailsExtensionNames.Code] = ProblemCodes.Validation.Failed;
            problem.Extensions[ProblemDetailsExtensionNames.TraceId] = TraceIdAccessor.GetTraceId(httpContext);

            return problem;
        }
    }
}
