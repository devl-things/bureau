using System.Text.RegularExpressions;

namespace Watson.Nodes.Abstractions.Conventions
{
    /// <summary>
    /// Defines and validates canonical conventions for scope.
    /// </summary>
    public static class ScopeConventions
    {
        private const int MaxScopeLength = 64;
        // lowercase, a-z 0-9 _ - :
        private static readonly Regex ScopeRegex = new Regex("^[a-z0-9:_-]{1,64}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));
        /// <summary>
        /// Normalizes a scope value to its canonical form.
        /// </summary>
        /// <remarks>
        /// Normalization performs minimal, safe transformations only:
        /// <list type="bullet">
        /// <item><description>trims leading and trailing whitespace</description></item>
        /// <item><description>converts to lowercase</description></item>
        /// </list>
        /// This method does NOT attempt to correct invalid formats.
        /// </remarks>
        public static string NormalizeScope(string scope)
        {
            return scope.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Validates whether a scope value conforms to the canonical convention.
        /// </summary>
        /// <param name="scope">Scope value to validate.</param>
        /// <returns>
        /// True if the scope is valid; otherwise false.
        /// </returns>
        public static bool IsValidScope(string scope)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                return false;
            }

            if (scope.Length > MaxScopeLength)
            {
                return false;
            }

            return ScopeRegex.IsMatch(scope);
        }

        /// <summary>
        /// Normalizes and validates a scope value.
        /// </summary>
        /// <param name="scope">Raw scope input.</param>
        /// <returns>The normalized scope.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the scope does not conform to the canonical convention.
        /// </exception>
        public static string NormalizeAndValidateScope(string scope)
        {
            string normalized = NormalizeScope(scope);

            if (!IsValidScope(normalized))
            {
                throw new ArgumentException(
                    "Invalid scope. Scopes must be lowercase, 1–64 characters long, " +
                    "and contain only a–z, 0–9, '_', '-', ':' characters.",
                    nameof(scope));
            }

            return normalized;
        }
    }
}
