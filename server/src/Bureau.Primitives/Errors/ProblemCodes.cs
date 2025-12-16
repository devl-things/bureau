namespace Bureau.Primitives.Errors
{
    public static class ProblemCodes
    {
        public static class Operation
        {
            public const string Failed = "operation.failed";
        }
        public static class Request
        {
            public const string InvalidId = "request.invalid_id";
            public const string IdMismatch = "request.id_mismatch";
        }
        public static class Resource
        {
            public const string NotFound = "resource.not_found";
            public const string Conflict = "resource.conflict";
        }
        public static class System
        {
            public const string UnexpectedError = "system.unexpected_error";
        }
        public static class Validation
        {
            public const string Failed = "validation.failed";
        }
    }
}
