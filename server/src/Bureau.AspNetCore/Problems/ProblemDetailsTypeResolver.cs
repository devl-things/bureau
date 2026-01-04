namespace Bureau.AspNetCore.Problems
{
    public static class ProblemDetailsTypeResolver
    {
        private const string UrnPrefix = "urn:bureau:problem:";

        public static string Resolve(string code)
        {
            return UrnPrefix + code;
        }
    }
}
