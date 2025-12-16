namespace Bureau.AspNetCore
{
    public static class ProblemDetailsType
    {
        private const string UrnPrefix = "urn:bureau:problem:";

        public static string FromCode(string code)
        {
            // code is expected to be stable and machine-readable
            // e.g. "system.unexpected_error"
            return UrnPrefix + code;
        }
    }
}
