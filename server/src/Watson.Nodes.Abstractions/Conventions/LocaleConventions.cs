using Bureau;
using Bureau.Primitives.Errors;
using System.Globalization;

namespace Watson.Nodes.Abstractions.Conventions
{
    /// <summary>
    /// Defines canonical conventions for locale values and provides validation helpers.
    /// </summary>
    /// <remarks>
    /// Locale values are optional and may be <c>null</c>, which represents an
    /// invariant (non-localized) attribute value.
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
    /// When provided, locale values must conform to BCP 47 language tag conventions.
    /// </remarks>
    public static class LocaleConventions
    {
        public const string FallbackLocale = "en";
        /// <summary>
        /// Normalizes a locale value to its canonical form.
        /// </summary>
        /// <remarks>
        /// Normalization performs minimal, safe transformations only:
        /// <list type="bullet">
        /// <item><description>trims leading and trailing whitespace</description></item>
        /// </list>
        /// The original casing is preserved, as BCP 47 casing (e.g. <c>en-US</c>)
        /// is meaningful and should not be altered.
        /// 
        /// This method does not perform validation and may return values that are
        /// not valid locale identifiers. Validation must be performed separately.
        /// </remarks>
        /// <param name="locale">Raw locale input.</param>
        /// <returns>
        /// The normalized locale value, or <c>null</c> if the input is <c>null</c>
        /// or consists only of whitespace.
        /// </returns>
        public static string? Normalize(string? locale)
        {
            if (string.IsNullOrWhiteSpace(locale))
            {
                return null;
            }

            return locale.Trim();
        }

        /// <summary>
        /// Validates a locale value against BCP 47 language tag conventions.
        /// </summary>
        /// <remarks>
        /// Locale values are optional:
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// A <c>null</c> value is considered valid and represents an invariant
        /// (non-localized) attribute.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// When provided, the locale must be a valid BCP 47 language tag.
        /// </description>
        /// </item>
        /// </list>
        /// 
        /// This method assumes the value has already been normalized.
        /// </remarks>
        /// <param name="locale">Locale value to validate.</param>
        /// <returns>
        /// A successful <see cref="Result"/> if the locale is valid; otherwise
        /// an error result describing the validation failure.
        /// </returns>
        public static Result Validate(string? locale)
        {
            if (locale is null)
            {
                return true;
            }

            try
            {
                // .NET validates BCP 47 language tags via CultureInfo
                CultureInfo.GetCultureInfo(locale);
                return true;
            }
            catch (CultureNotFoundException ex)
            {
                return ResultError.From(
                    ProblemCodes.Validation.InvalidFormat,
                    "Invalid locale. Locale values must be valid BCP 47 language tags (e.g. 'en', 'en-US', 'hr', 'hr-HR').",
                    ex);
            }
        }

        /// <summary>
        /// Computes a preference score for a candidate locale relative to a requested locale.
        /// </summary>
        /// <remarks>
        /// This method is used to select the best matching localized attribute when multiple
        /// locale variants exist for the same attribute key.
        ///
        /// Scoring rules (higher is better):
        /// <list type="bullet">
        /// <item>
        /// <description>
        /// Exact match with the requested locale → highest priority.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Invariant locale (<c>null</c>) → used as a generic fallback when no exact match exists.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Fallback locale (e.g. <c>en</c>) → lowest-priority explicit fallback.
        /// </description>
        /// </item>
        /// <item>
        /// <description>
        /// Any other locale → not preferred.
        /// </description>
        /// </item>
        /// </list>
        ///
        /// The returned score is only meaningful when compared against other scores produced
        /// by this method for the same requested locale.
        /// </remarks>
        /// <param name="locale">
        /// The locale of the candidate attribute value. A <c>null</c> value represents
        /// an invariant (non-localized) attribute.
        /// </param>
        /// <param name="requestedLocale">
        /// The locale requested by the caller. Must be a normalized, valid locale value.
        /// </param>
        /// <returns>
        /// An integer preference score where higher values indicate a better locale match.
        /// </returns>
        public static int GetLocaleScore(string? locale, string requestedLocale)
        {
            if (locale == requestedLocale)
            {
                return 3;
            }

            if (locale == null)
            {
                return 2;
            }

            if (locale == FallbackLocale)
            {
                return 1;
            }

            return 0;
        }
    }
}
