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
            ValidationProblemDetails problem = new(modelState);
            PopulateDefaults(problem, httpContext);
            return problem;
        }

        public static ValidationProblemDetails Create(HttpContext httpContext, IDictionary<string, string[]> errors)
        {
            ValidationProblemDetails problem = new(errors);
            PopulateDefaults(problem, httpContext);
            return problem;
        }

        private static void PopulateDefaults(ValidationProblemDetails problem, HttpContext httpContext)
        {
            string code = ProblemCodes.Validation.Failed;
            ProblemCodeDefinition pcDefinition = ProblemCodeDefinitionResolver.Resolve(code);

            problem.Status = pcDefinition.StatusCode;
            problem.Title = pcDefinition.Title;
            problem.Instance = httpContext.Request.Path;
            problem.Type = ProblemDetailsTypeResolver.Resolve(code);

            problem.Extensions[ProblemDetailsExtensionNames.Code] = code;
            problem.Extensions[ProblemDetailsExtensionNames.TraceId] = TraceIdAccessor.GetTraceId(httpContext);
        }
    }
}
