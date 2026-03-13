using Bureau;
using Bureau.Primitives.Errors;

namespace Watson.Nodes.Abstractions.Conventions
{
    /// <summary>
    /// Defines normalization and validation rules for node canonical keys.
    /// A canonical key is a stable, human-readable identifier used for
    /// uniqueness, lookup, and cross-system references.
    /// </summary>
    public static class CanonicalKeyConventions
    {
        /// <summary>
        /// The maximum allowed length of a canonical key.
        /// </summary>
        public const int MaxCanonicalKeyLength = 256;

        /// <summary>
        /// Normalizes a canonical key by trimming whitespace.
        /// Returns <c>null</c> if the input is <c>null</c>, empty, or whitespace.
        /// </summary>
        /// <param name="canonicalKey">
        /// The canonical key value to normalize.
        /// </param>
        /// <returns>
        /// A trimmed canonical key, or <c>null</c> if the input is empty or whitespace.
        /// </returns>
        public static string? Normalize(string? canonicalKey)
        {
            if (string.IsNullOrWhiteSpace(canonicalKey))
            {
                return null;
            }

            return canonicalKey.Trim();
        }

        /// <summary>
        /// Validates a canonical key against length constraints.
        /// An empty or <c>null</c> value is considered valid.
        /// </summary>
        /// <param name="canonicalKey">
        /// The canonical key to validate.
        /// </param>
        /// <returns>
        /// A successful <see cref="Result"/> if the key is valid; otherwise,
        /// a validation error indicating the reason for failure.
        /// </returns>
        public static Result Validate(string? canonicalKey)
        {
            if (string.IsNullOrWhiteSpace(canonicalKey)) return true;
            if (canonicalKey.Length > MaxCanonicalKeyLength)
            {
                return ResultError.From(ProblemCodes.Validation.TooLong, $"Invalid canonical key length. Canonical key must be 0–{MaxCanonicalKeyLength} characters long.");
            }
            return true;
        }

        /// <summary>
        /// Ensures that a canonical key is provided for node kinds
        /// that require one.
        /// </summary>
        /// <param name="canonicalKey">
        /// The canonical key value to check.
        /// </param>
        /// <param name="nodeKindName">
        /// The name of the node kind requiring the canonical key.
        /// </param>
        /// <returns>
        /// A successful <see cref="Result"/> if the canonical key is present;
        /// otherwise, a validation error indicating the key is required.
        /// </returns>
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
