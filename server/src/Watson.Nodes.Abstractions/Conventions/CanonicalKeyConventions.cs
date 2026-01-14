using Bureau;
using Bureau.Primitives.Errors;

namespace Watson.Nodes.Abstractions.Conventions
{
    //TODO add XML docs
    public static class CanonicalKeyConventions
    {
        public const int MaxCanonicalKeyLength = 256;

        public static string? Normalize(string? canonicalKey)
        {
            if (string.IsNullOrWhiteSpace(canonicalKey))
            {
                return null;
            }

            return canonicalKey.Trim();
        }

        public static Result Validate(string? canonicalKey)
        {
            if (string.IsNullOrWhiteSpace(canonicalKey)) return true;
            if (canonicalKey.Length > MaxCanonicalKeyLength)
            {
                return ResultError.From(ProblemCodes.Validation.TooLong, $"Invalid canonical key length. Canonical key must be 0–{MaxCanonicalKeyLength} characters long.");
            }
            return true;
        }

        public static Result RequireCanonicalKey(string? canonicalKey, string nodeKindName)
        {
            if (string.IsNullOrWhiteSpace(canonicalKey))
            {
                return ResultError.From(ProblemCodes.Validation.Required, $"{nameof(canonicalKey)} is required", $"{nameof(canonicalKey)} is required for " + nodeKindName + " nodes.");
            }

            return true;
        }
    }
}
