using Bureau;
using Bureau.Primitives.Errors;
using System.Text.RegularExpressions;

namespace Watson.Nodes.Abstractions.Conventions
{
    public static class EdgeConventions
    {
        public const int MaxPurposeLength = 64;

        private static readonly Regex PurposeRegex =
            new Regex("^[a-z0-9_-]{1,64}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        //TODO documentation
        public static string? Normalize(string? purpose)
        {
            if (string.IsNullOrWhiteSpace(purpose))
            {
                return null;
            }

            return purpose.Trim().ToLowerInvariant();
        }

        public static Result Validate(string? purpose)
        {
            if (string.IsNullOrWhiteSpace(purpose))
            {
                return ResultError.From(ProblemCodes.Validation.Required, "Purpose is required.");
            }

            if (purpose.Length > MaxPurposeLength)
            {
                return ResultError.From(ProblemCodes.Validation.TooLong, $"Invalid purpose length. Purpose must be 1\u2013{MaxPurposeLength} characters long.");
            }

            if (!PurposeRegex.IsMatch(purpose))
            {
                return ResultError.From(ProblemCodes.Validation.InvalidFormat,
                    $"Invalid purpose. Purpose must be lowercase, 1\u2013{MaxPurposeLength} characters long, and contain only a\u2013z, 0\u20139, '_', '-' characters.");
            }

            return true;
        }

        public static Result ValidateSourceTargetDifferent(Guid sourceNodeId, Guid targetNodeId)
        {
            if (sourceNodeId == targetNodeId)
            {
                return ResultError.From(ProblemCodes.Validation.InvalidFormat, "Source and target node must be different.");
            }

            return true;
        }
    }
}
