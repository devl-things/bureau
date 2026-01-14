using Bureau;
using Bureau.Primitives.Errors;
using System.Text.RegularExpressions;

namespace Watson.Nodes.Abstractions.Conventions
{
    /// <summary>
    /// Defines canonical conventions for scope values and provides validation helpers.
    /// </summary>
    /// <remarks>
    /// Scope values identify logical ownership or partitioning of nodes
    /// (e.g. global, tenant-specific, project-specific).
    ///
    /// Normalization and validation are intentionally separated:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Normalization is typically performed at API or mapping boundaries
    /// (e.g. DTO → domain).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Validation is performed in the service layer to enforce canonical rules.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// Canonical scope rules:
    /// <list type="bullet">
    /// <item><description>lowercase</description></item>
    /// <item><description>allowed characters: <c>a-z</c>, <c>0-9</c>, <c>_</c>, <c>-</c>, <c>:</c></description></item>
    /// <item><description>length: 1–64 characters</description></item>
    /// </list>
    /// </remarks>
    public static class ScopeConventions
    {
        public const int MaxScopeLength = 64;

        // lowercase, a-z 0-9 _ - :
        private static readonly Regex ScopeRegex =
            new Regex("^[a-z0-9:_-]{1,64}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        /// <summary>
        /// Normalizes a scope value to its canonical form.
        /// </summary>
        /// <remarks>
        /// Normalization performs minimal, safe transformations only:
        /// <list type="bullet">
        /// <item><description>trims leading and trailing whitespace</description></item>
        /// <item><description>converts to lowercase</description></item>
        /// </list>
        /// This method does not perform validation and may return values that do not
        /// conform to the canonical convention. Validation must be performed separately.
        /// </remarks>
        /// <param name="scope">Raw scope input.</param>
        /// <returns>
        /// The normalized scope value, or <c>null</c> if the input is <c>null</c>
        /// or consists only of whitespace.
        /// </returns>
        public static string? Normalize(string? scope)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                return null;
            }

            return scope.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Validates whether a scope value conforms to the canonical convention.
        /// </summary>
        /// <remarks>
        /// This method assumes the input has already been normalized.
        /// A <c>null</c> value is considered invalid for scope.
        /// </remarks>
        /// <param name="scope">Scope value to validate (expected normalized).</param>
        /// <returns>
        /// A successful <see cref="Result"/> if the scope is valid; otherwise
        /// an error result describing the validation failure.
        /// </returns>
        public static Result Validate(string? scope)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                return ResultError.From(ProblemCodes.Validation.Required, "Scope is required.");
            }

            if (scope.Length > MaxScopeLength)
            {
                return ResultError.From(ProblemCodes.Validation.TooLong, $"Invalid scope length. Scopes must be 1–{MaxScopeLength} characters long.");
            }

            if (!ScopeRegex.IsMatch(scope))
            {
                return ResultError.From(ProblemCodes.Validation.InvalidFormat,
                    $"Invalid scope. Scopes must be lowercase, 1–{MaxScopeLength} characters long, and contain only a–z, 0–9, '_', '-', ':' characters.");
            }

            return true;
        }

        /// <summary>
        /// Normalizes a scope value and validates the normalized result.
        /// </summary>
        /// <remarks>
        /// Convenience helper for cases where normalization and validation occur together.
        /// Prefer calling <see cref="Normalize"/> in mapping and <see cref="Validate"/> in
        /// the service layer when enforcing strict boundaries.
        /// </remarks>
        /// <param name="scope">Raw scope input.</param>
        /// <returns>
        /// A successful <see cref="Result{T}"/> containing the normalized scope;
        /// otherwise an error result.
        /// </returns>
        public static Result<string> NormalizeAndValidate(string? scope)
        {
            string? normalized = Normalize(scope);

            Result validation = Validate(normalized);
            if (validation.IsError)
            {
                return validation.Error;
            }

            return normalized!;
        }
    }
}
