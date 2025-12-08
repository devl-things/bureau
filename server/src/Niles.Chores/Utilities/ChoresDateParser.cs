using System.Globalization;

namespace Niles.Chores
{
    public static class ChoresDateParser
    {
        // Centralized formats
        public const string DateFormat = "yyyy-MM-dd";                 // e.g. 2025-02-10
        public const string DateTimeFormatWithOffset = "yyyy-MM-dd'T'HH:mm:ss.fffK";  // e.g. 2025-02-10T14:30:00Z for UTC or 2025-02-10T14:30:00-05:00 for offset
        public const string DurationFormat_HH_MM = "hh\\:mm";  // e.g. 01:30
        public const string DurationFormat_H_MM = "h\\:mm";

        // Centralized culture for all date/time parsing and formatting
        public static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        public static bool TryParseDate(string? value, out DateOnly dateOnly)
        {
            return DateOnly.TryParseExact(value, DateFormat, Culture, DateTimeStyles.None, out dateOnly);
        }

        /// <summary>
        /// Attempts to parse a datetime string into a <see cref="DateTimeOffset"/> using standardized,
        /// unambiguous formats. Supports either a full ISO 8601 datetime with offset
        /// (<c>yyyy-MM-dd'T'HH:mm:ss.fffK</c>) or a date-only value (<c>yyyy-MM-dd</c>).
        /// <para>
        /// If a full datetime with offset is provided, the offset is respected and the value is
        /// normalized to UTC. If only a date is provided, it is interpreted as midnight UTC.
        /// </para>
        /// <para>
        /// All parsing uses <see cref="CultureInfo.InvariantCulture"/> and
        /// <see cref="DateTimeStyles.AssumeUniversal"/> combined with
        /// <see cref="DateTimeStyles.AdjustToUniversal"/> to guarantee that the result
        /// represents a single, unambiguous instant in time.
        /// </para>
        /// </summary>
        /// <param name="value">The input string to parse.</param>
        /// <param name="dateTime">When successful, receives the parsed UTC-normalized timestamp.</param>
        /// <returns>
        /// <c>true</c> if the input matches one of the supported formats; otherwise <c>false</c>.
        /// </returns>
        public static bool TryParseDateTime(string? value, out DateTimeOffset dateTime)
        {
            string[] formats = [DateTimeFormatWithOffset, DateFormat];

            return DateTimeOffset.TryParseExact(value, formats, Culture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out dateTime);
        }

        /// <summary>
        /// Attempts to parse a duration string into a <see cref="TimeSpan"/>.
        /// <para>
        /// Supported formats:
        /// <list type="bullet">
        /// <item><description><c>hh:mm</c> – two-digit hours and minutes (e.g. "01:30")</description></item>
        /// <item><description><c>h:mm</c> – one-digit hours and minutes (e.g. "1:30")</description></item>
        /// <item><description><c>mm</c> – total minutes as an integer (e.g. "90")</description></item>
        /// </list>
        /// Whitespace is ignored. Hours and minutes are validated against the format rules.
        /// Total minutes must be a non-negative integer.
        /// </para>
        /// </summary>
        /// <param name="value">The input duration string to parse.</param>
        /// <param name="duration">When successful, receives the parsed <see cref="TimeSpan"/>.</param>
        /// <returns>
        /// <c>true</c> if parsing succeeds using one of the supported formats; otherwise <c>false</c>.
        /// </returns>
        public static bool TryParseDuration(string? value, out TimeSpan duration)
        {
            duration = TimeSpan.Zero;

            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string trimmed = value.Trim();

            // --- Case 1: time-style formats ("hh:mm", "h:mm") ---
            string[] durationFormats = [DurationFormat_HH_MM, DurationFormat_H_MM]; // defined as constants below

            if (trimmed.Contains(":", StringComparison.Ordinal) &&
                TimeSpan.TryParseExact(trimmed, durationFormats, Culture, TimeSpanStyles.None, out duration))
            {
                return true;
            }

            // --- Case 2: minutes-only format ("mm") ---
            if (int.TryParse(trimmed, NumberStyles.Integer, Culture, out int totalMinutes) &&
                totalMinutes >= 0)
            {
                duration = TimeSpan.FromMinutes(totalMinutes);
                return true;
            }

            return false;
        }
    }
}
