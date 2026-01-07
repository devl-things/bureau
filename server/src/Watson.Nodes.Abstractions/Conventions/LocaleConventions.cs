using System.Globalization;

namespace Watson.Nodes.Abstractions.Conventions
{
    /// <summary>
    /// Defines and validates canonical conventions for locale.
    /// </summary>
    public static class LocaleConventions
    {
        /// <summary>
        /// Normalizes a locale value to its canonical form.
        /// </summary>
        /// <remarks>
        /// Normalization trims whitespace but preserves case, as BCP 47 casing
        /// (e.g. <c>en-US</c>) is meaningful.
        /// </remarks>
        public static string NormalizeLocale(string locale)
        {
            return locale.Trim();
        }

        /// <summary>
        /// Validates whether a locale value is a valid BCP 47 language tag.
        /// </summary>
        /// <param name="locale">Locale value to validate.</param>
        /// <returns>
        /// True if the locale is a valid BCP 47 language tag; otherwise false.
        /// </returns>
        public static bool IsValidLocale(string locale)
        {
            if (string.IsNullOrWhiteSpace(locale))
            {
                return false;
            }

            try
            {
                // .NET uses BCP 47 internally
                CultureInfo.GetCultureInfo(locale);
                return true;
            }
            catch (CultureNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Normalizes and validates a locale value.
        /// </summary>
        /// <param name="locale">Raw locale input.</param>
        /// <returns>The normalized locale.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the locale is not a valid BCP 47 language tag.
        /// </exception>
        public static string NormalizeAndValidateLocale(string locale)
        {
            string normalized = NormalizeLocale(locale);

            if (!IsValidLocale(normalized))
            {
                throw new ArgumentException(
                    "Invalid locale. Locale values must be valid BCP 47 language tags " +
                    "(e.g. 'en', 'en-US', 'hr', 'hr-HR').",
                    nameof(locale));
            }

            return normalized;
        }
    }
}
