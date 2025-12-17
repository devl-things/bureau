using Bureau.Primitives.Errors;
using Microsoft.AspNetCore.Http;

namespace Bureau.AspNetCore.Problems
{
    public static class ProblemCodeDefinitionResolver
    {
        public static readonly IReadOnlyDictionary<string, ProblemCodeDefinition> ProblemCodeDefinitionByCode = new Dictionary<string, ProblemCodeDefinition>
        {
            // ======================
            // Validation (422)
            // ======================
            [ProblemCodes.Validation.Failed] = new ProblemCodeDefinition(StatusCodes.Status422UnprocessableEntity, "Validation failed."),
            [ProblemCodes.Validation.Required] = new ProblemCodeDefinition(StatusCodes.Status422UnprocessableEntity, "Required value is missing."),
            [ProblemCodes.Validation.InvalidFormat] = new ProblemCodeDefinition(StatusCodes.Status422UnprocessableEntity, "Invalid value format."),
            [ProblemCodes.Validation.OutOfRange] = new ProblemCodeDefinition(StatusCodes.Status422UnprocessableEntity, "Value is out of allowed range."),
            [ProblemCodes.Validation.TooShort] = new ProblemCodeDefinition(StatusCodes.Status422UnprocessableEntity, "Value is too short."),
            [ProblemCodes.Validation.TooLong] = new ProblemCodeDefinition(StatusCodes.Status422UnprocessableEntity, "Value is too long."),
            [ProblemCodes.Validation.NotUnique] = new ProblemCodeDefinition(StatusCodes.Status409Conflict, "Value must be unique."),
            [ProblemCodes.Validation.InvalidEnumValue] = new ProblemCodeDefinition(StatusCodes.Status422UnprocessableEntity, "Invalid value."),

            // ======================
            // Request
            // ======================
            [ProblemCodes.Request.Failed] = new ProblemCodeDefinition(StatusCodes.Status400BadRequest, "Request failed."),
            [ProblemCodes.Request.InvalidId] = new ProblemCodeDefinition(StatusCodes.Status400BadRequest, "Invalid identifier."),
            [ProblemCodes.Request.IdMismatch] = new ProblemCodeDefinition(StatusCodes.Status400BadRequest, "Request identifiers do not match."),
            [ProblemCodes.Request.InvalidPathParam] = new ProblemCodeDefinition(StatusCodes.Status400BadRequest, "Invalid path parameter."),
            [ProblemCodes.Request.InvalidQueryParam] = new ProblemCodeDefinition(StatusCodes.Status400BadRequest, "Invalid query parameter."),

            [ProblemCodes.Request.InvalidJson] = new ProblemCodeDefinition(StatusCodes.Status400BadRequest, "Malformed JSON payload."),
            [ProblemCodes.Request.InvalidPayload] = new ProblemCodeDefinition(StatusCodes.Status400BadRequest, "Invalid request payload."),
            [ProblemCodes.Request.MissingRequiredField] = new ProblemCodeDefinition(StatusCodes.Status400BadRequest, "Missing required field."),

            [ProblemCodes.Request.Unauthenticated] = new ProblemCodeDefinition(StatusCodes.Status401Unauthorized, "Authentication required."),
            [ProblemCodes.Request.Forbidden] = new ProblemCodeDefinition(StatusCodes.Status403Forbidden, "Access denied."),
            [ProblemCodes.Request.MethodNotAllowed] = new ProblemCodeDefinition(StatusCodes.Status405MethodNotAllowed, "HTTP method not allowed."),
            [ProblemCodes.Request.UnsupportedMediaType] = new ProblemCodeDefinition(StatusCodes.Status415UnsupportedMediaType, "Unsupported media type."),
            [ProblemCodes.Request.NotAcceptable] = new ProblemCodeDefinition(StatusCodes.Status406NotAcceptable, "Requested representation is not acceptable."),

            [ProblemCodes.Request.RateLimited] = new ProblemCodeDefinition(StatusCodes.Status429TooManyRequests, "Too many requests."),
            [ProblemCodes.Request.PayloadTooLarge] = new ProblemCodeDefinition(StatusCodes.Status413PayloadTooLarge, "Request payload is too large."),

            // ======================
            // Resource
            // ======================
            [ProblemCodes.Resource.NotFound] = new ProblemCodeDefinition(StatusCodes.Status404NotFound, "Resource not found."),
            [ProblemCodes.Resource.Gone] = new ProblemCodeDefinition(StatusCodes.Status410Gone, "Resource is no longer available."),


            [ProblemCodes.Resource.Conflict] = new ProblemCodeDefinition(StatusCodes.Status409Conflict, "Resource conflict."),
            [ProblemCodes.Resource.AlreadyExists] = new ProblemCodeDefinition(StatusCodes.Status409Conflict, "Resource already exists."),
            [ProblemCodes.Resource.VersionConflict] = new ProblemCodeDefinition(StatusCodes.Status409Conflict, "Resource version conflict."),

            [ProblemCodes.Resource.Locked] = new ProblemCodeDefinition(StatusCodes.Status423Locked, "Resource is locked."),
            [ProblemCodes.Resource.PreconditionFailed] = new ProblemCodeDefinition(StatusCodes.Status412PreconditionFailed, "Precondition failed."),

            // ======================
            // Operation
            // ======================
            [ProblemCodes.Operation.Failed] = new ProblemCodeDefinition(StatusCodes.Status500InternalServerError, "Operation failed."),
            [ProblemCodes.Operation.NotAllowed] = new ProblemCodeDefinition(StatusCodes.Status403Forbidden, "Operation is not allowed."),
            [ProblemCodes.Operation.InvalidState] = new ProblemCodeDefinition(StatusCodes.Status409Conflict, "Invalid operation state."),

            [ProblemCodes.Operation.DependencyFailed] = new ProblemCodeDefinition(StatusCodes.Status424FailedDependency, "Dependent operation failed."),
            [ProblemCodes.Operation.Timeout] = new ProblemCodeDefinition(StatusCodes.Status504GatewayTimeout, "Operation timed out."),
            [ProblemCodes.Operation.UnexpectedError] = new ProblemCodeDefinition(StatusCodes.Status500InternalServerError, "An unexpected operation error occurred."),

            // ======================
            // System
            // ======================
            [ProblemCodes.System.UnexpectedError] = new ProblemCodeDefinition(StatusCodes.Status500InternalServerError, "An unexpected system error occurred."),
            [ProblemCodes.System.ServiceUnavailable] = new ProblemCodeDefinition(StatusCodes.Status503ServiceUnavailable, "Service is unavailable."),
            [ProblemCodes.System.ConfigurationError] = new ProblemCodeDefinition(StatusCodes.Status500InternalServerError, "System configuration error.")
        };


        private static readonly IReadOnlyDictionary<int, string> DefaultCodeByStatus = new Dictionary<int, string>
        {
            [StatusCodes.Status400BadRequest] = ProblemCodes.Request.Failed,
            [StatusCodes.Status401Unauthorized] = ProblemCodes.Request.Unauthenticated,
            [StatusCodes.Status403Forbidden] = ProblemCodes.Request.Forbidden,
            [StatusCodes.Status404NotFound] = ProblemCodes.Resource.NotFound,
            [StatusCodes.Status405MethodNotAllowed] = ProblemCodes.Request.MethodNotAllowed,
            [StatusCodes.Status406NotAcceptable] = ProblemCodes.Request.NotAcceptable,
            [StatusCodes.Status409Conflict] = ProblemCodes.Resource.Conflict,
            [StatusCodes.Status410Gone] = ProblemCodes.Resource.Gone,
            [StatusCodes.Status412PreconditionFailed] = ProblemCodes.Resource.PreconditionFailed,
            [StatusCodes.Status413PayloadTooLarge] = ProblemCodes.Request.PayloadTooLarge,
            [StatusCodes.Status415UnsupportedMediaType] = ProblemCodes.Request.UnsupportedMediaType,
            [StatusCodes.Status422UnprocessableEntity] = ProblemCodes.Validation.Failed,
            [StatusCodes.Status423Locked] = ProblemCodes.Resource.Locked,
            [StatusCodes.Status424FailedDependency] = ProblemCodes.Operation.DependencyFailed,
            [StatusCodes.Status429TooManyRequests] = ProblemCodes.Request.RateLimited,
            [StatusCodes.Status500InternalServerError] = ProblemCodes.System.UnexpectedError,
            [StatusCodes.Status503ServiceUnavailable] = ProblemCodes.System.ServiceUnavailable,
            [StatusCodes.Status504GatewayTimeout] = ProblemCodes.Operation.Timeout
        };
        public static ProblemCodeDefinition Resolve(string? code)
        {
            if (string.IsNullOrWhiteSpace(code)) return ProblemCodeDefinitionByCode[ProblemCodes.System.ConfigurationError];
            if (ProblemCodeDefinitionByCode.TryGetValue(code, out ProblemCodeDefinition? def))
            {
                return def;
            }
            return ProblemCodeDefinitionByCode[ProblemCodes.System.ConfigurationError];
        }

        public static ProblemCodeDefinition Resolve(int statusCode)
        {
            string? code = DefaultCodeByStatus.GetValueOrDefault(statusCode);
            return Resolve(code);
        }
    }
    public sealed class ProblemCodeDefinition
    {
        public int StatusCode { get; }
        public string Title { get; }

        public ProblemCodeDefinition(int statusCode, string title)
        {
            StatusCode = statusCode;
            Title = title;
        }
    }
}