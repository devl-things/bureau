using System.Text.RegularExpressions;

namespace Watson.Nodes.Conventions
{
    /// <summary>
    /// Defines and validates canonical conventions for node attribute keys.
    /// </summary>
    public static class NodeAttributeConventions
    {
        private const int MaxKeyLength = 64;

        // lowercase snake_case with optional namespace (e.g. catalog:ean)
        private static readonly Regex AttributeKeyRegex = new Regex("^[a-z0-9:_]{1,64}$", RegexOptions.Compiled, TimeSpan.FromMilliseconds(100));

        /// <summary>
        /// Normalizes an attribute key to its canonical form.
        /// </summary>
        /// <remarks>
        /// Normalization performs minimal, safe transformations:
        /// <list type="bullet">
        /// <item><description>trims leading/trailing whitespace</description></item>
        /// <item><description>converts to lowercase</description></item>
        /// </list>
        /// This method does NOT attempt to fix invalid formats (e.g. spaces, camelCase).
        /// </remarks>
        public static string NormalizeAttributeKey(string key)
        {
            return key.Trim().ToLowerInvariant();
        }

        /// <summary>
        /// Validates whether an attribute key conforms to the canonical convention.
        /// </summary>
        /// <param name="key">The attribute key to validate.</param>
        /// <returns>
        /// True if the key is valid according to convention; otherwise false.
        /// </returns>
        public static bool IsValidAttributeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            if (key.Length > MaxKeyLength)
            {
                return false;
            }

            return AttributeKeyRegex.IsMatch(key);
        }

        /// <summary>
        /// Normalizes and validates an attribute key.
        /// </summary>
        /// <param name="key">Raw attribute key input.</param>
        /// <returns>The normalized attribute key.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the key does not conform to the canonical convention.
        /// </exception>
        public static string NormalizeAndValidateAttributeKey(string key)
        {
            string normalized = NormalizeAttributeKey(key);

            if (!IsValidAttributeKey(normalized))
            {
                throw new ArgumentException(
                    "Invalid attribute key. Keys must be lowercase snake_case, " +
                    "1–64 characters long, and contain only a–z, 0–9, '_', ':' characters.",
                    nameof(key));
            }

            return normalized;
        }
    }
}
