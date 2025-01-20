namespace Bureau.Data.Postgres.Models
{
    internal class OccurrenceRecord
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Start date and time of the period (in ISO 8601 format).
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date and time of the period (in ISO 8601 format). Optional.
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Frequency of recurrence (e.g., DAILY, WEEKLY, MONTHLY, YEARLY).
        /// </summary>
        public int? Frequency { get; set; }

        /// <summary>
        /// Interval between recurrences. Defaults to 1.
        /// </summary>
        public int Interval { get; set; }

        /// <summary>
        /// Specific days of the week for recurrence (e.g., MO, TU), stored as a comma-separated string.
        /// </summary>
        public string ByDay { get; set; } = string.Empty;

        /// <summary>
        /// Specific days of the month for recurrence, stored as a comma-separated string.
        /// </summary>
        public string ByMonthDay { get; set; } = string.Empty;

        /// <summary>
        /// Specific days of the year for recurrence, stored as a comma-separated string.
        /// </summary>
        public string ByYearDay { get; set; } = string.Empty;

        /// <summary>
        /// Specific weeks of the year for recurrence, stored as a comma-separated string.
        /// </summary>
        public string ByWeekNo { get; set; } = string.Empty;

        /// <summary>
        /// Specific months of the year for recurrence, stored as a comma-separated string.
        /// </summary>
        public string ByMonth { get; set; } = string.Empty;

        /// <summary>
        /// Specifies occurrences based on their position in the recurrence set, stored as a comma-separated string.
        /// </summary>
        public string BySetPos { get; set; } = string.Empty;

        /// <summary>
        /// End date of the recurrence (in ISO 8601 format). Exclusive with Count.
        /// </summary>
        public DateTime? Until { get; set; }

        /// <summary>
        /// Number of occurrences of the recurrence. Exclusive with Until.
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        /// Week start day for the recurrence (e.g., Monday). Defaults to Monday.
        /// </summary>
        public int WeekStart { get; set; }

        public Record Record { get; set; } = null!; // Navigation Property
    }
}
