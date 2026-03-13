namespace Bureau.Primitives.Errors
{
    public static class ProblemCodes
    {
        public static class Operation
        {
            public const string Failed = "operation.failed";
            public const string NotAllowed = "operation.not_allowed";
            public const string InvalidState = "operation.invalid_state";

            public const string DependencyFailed = "operation.dependency_failed";
            public const string Timeout = "operation.timeout";

            public const string UnexpectedError = "operation.unexpected_error";
        }

        public static class Request
        {
            public const string Failed = "request.failed";

            public const string InvalidId = "request.invalid_id";
            public const string IdMismatch = "request.id_mismatch";
            public const string InvalidPathParam = "request.invalid_path_param";
            public const string InvalidQueryParam = "request.invalid_query_param";

            public const string InvalidJson = "request.invalid_json";
            public const string InvalidPayload = "request.invalid_payload";
            public const string MissingRequiredField = "request.missing_required_field";

            public const string Unauthenticated = "request.unauthenticated";
            public const string Forbidden = "request.forbidden";
            public const string MethodNotAllowed = "request.method_not_allowed";
            public const string UnsupportedMediaType = "request.unsupported_media_type";
            public const string NotAcceptable = "request.not_acceptable";

            public const string RateLimited = "request.rate_limited";
            public const string PayloadTooLarge = "request.payload_too_large";
        }

        public static class Resource
        {
            public const string NotFound = "resource.not_found";
            public const string Gone = "resource.gone";

            public const string Conflict = "resource.conflict";
            public const string AlreadyExists = "resource.already_exists";
            public const string VersionConflict = "resource.version_conflict";
            public const string Locked = "resource.locked";
            public const string PreconditionFailed = "resource.precondition_failed";
        }

        public static class Validation
        {
            public const string Failed = "validation.failed";

            public const string Required = "validation.required";
            public const string InvalidFormat = "validation.invalid_format";
            public const string OutOfRange = "validation.out_of_range";
            public const string TooShort = "validation.too_short";
            public const string TooLong = "validation.too_long";
            public const string NotUnique = "validation.not_unique";
            public const string InvalidEnumValue = "validation.invalid_enum_value";
        }

        public static class System
        {
            public const string UnexpectedError = "system.unexpected_error";
            public const string ServiceUnavailable = "system.service_unavailable";
            public const string ConfigurationError = "system.configuration_error";
        }
    }
}
