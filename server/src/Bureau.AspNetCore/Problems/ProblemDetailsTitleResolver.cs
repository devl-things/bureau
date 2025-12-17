using Bureau.Primitives.Errors;
using Microsoft.AspNetCore.Http;

namespace Bureau.AspNetCore.Problems
{
    internal static class ProblemDetailsTitleResolver
    {
        public static readonly IReadOnlyDictionary<string, string> TitlesByCode = new Dictionary<string, string>
        {
            [ProblemCodes.Validation.Failed] = "Validation failed.",
            [ProblemCodes.Request.Failed] = "Request failed.",
            [ProblemCodes.Request.InvalidId] = "Invalid identifier.",
            [ProblemCodes.Request.IdMismatch] = "Request is inconsistent.",
            [ProblemCodes.Resource.NotFound] = "Resource not found.",
            [ProblemCodes.Resource.Conflict] = "Resource conflict.",
            [ProblemCodes.Operation.Failed] = "Operation failed.",
            [ProblemCodes.System.UnexpectedError] = "An unexpected error occurred."
        };

        public static string Resolve(string code)
        {
            if (TitlesByCode.TryGetValue(code, out string? title))
            {
                return title;
            }
            return TitlesByCode[ProblemCodes.Request.Failed];
        }

        public static string Resolve(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => TitlesByCode[ProblemCodes.Validation.Failed],
                StatusCodes.Status500InternalServerError => TitlesByCode[ProblemCodes.System.UnexpectedError],
                _ => TitlesByCode[ProblemCodes.Request.Failed]
            };
        }
    }
}
