namespace Bureau.Core.Models
{
    /// <summary>
    /// Represents a OccurrenceRecord, combining a period of time with recurrence rules.
    /// Complies with RFC 5545 Sections 3.3.9 and 3.3.10.
    /// </summary>
    public class OccurrenceRecord : BaseRecord
    {
        /// <summary>
        /// Start date and time of the period (in ISO 8601 format).
        /// </summary>
        public DateTime Start { get; set; }

        public bool IsAllDay
        {
            get
            {
                return !End.HasValue;
            }
        }

        /// <summary>
        /// End date and time of the period (in ISO 8601 format). Optional if Duration is provided.
        /// </summary>
        public DateTime? End { get; set; }

        // TODO [future improvement]
        /// <summary>
        /// Duration of the period (in ISO 8601 duration format). Optional if End is provided.
        /// </summary>
        //public string Duration { get; set; }

        public bool IsSingle
        {
            get
            {
                return !Recurrence.HasValue;
            }
        }
        /// <summary>
        /// Recurrence rule details.
        /// </summary>
        public RecurrenceRule? Recurrence { get; set; }
    }

    /// <summary>
    /// Represents a recurrence rule as defined in RFC 5545 Section 3.3.10.
    /// </summary>
    public struct RecurrenceRule
    {
        /// <summary>
        /// Frequency of recurrence (e.g., DAILY, WEEKLY, MONTHLY, YEARLY).
        /// </summary>
        public RecurrenceFrequency Frequency { get; set; } 

        /// <summary>
        /// Interval between recurrences. Defaults to 1.
        /// </summary>
        public int Interval { get; set; } = 1;

        /// <summary>
        /// Specific days of the week for recurrence (e.g., MO, TU).
        /// </summary>
        public List<string> ByDay { get; set; } = new List<string>();

        /// <summary>
        /// Specific days of the month for recurrence (e.g., 1, -1).
        /// </summary>
        public List<int> ByMonthDay { get; set; } = new List<int>();

        /// <summary>
        /// Specific days of the year for recurrence (e.g., 1, 365, -1).
        /// </summary>
        public List<int> ByYearDay { get; set; } = new List<int>();

        /// <summary>
        /// Specific weeks of the year for recurrence (e.g., 1, 52).
        /// </summary>
        public List<int> ByWeekNo { get; set; } = new List<int>();

        /// <summary>
        /// Specific months of the year for recurrence (e.g., 1 for January, 12 for December).
        /// </summary>
        public List<int> ByMonth { get; set; } = new List<int>();

        /// <summary>
        /// Specifies occurrences based on their position in the recurrence set (e.g., 1 for the first occurrence).
        /// </summary>
        public List<int> BySetPos { get; set; } = new List<int>();

        /// <summary>
        /// End date of the recurrence (in ISO 8601 format). Exclusive with Count.
        /// </summary>
        public DateTime? Until { get; set; }

        /// <summary>
        /// Number of occurrences of the recurrence. Exclusive with Until.
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        /// Week start day for the recurrence (e.g., MO for Monday). Defaults to Monday.
        /// </summary>
        public DayOfWeek WeekStart { get; set; } = DayOfWeek.Monday;
        public RecurrenceRule()
        {
        }
    }

    public enum RecurrenceFrequency
    {
        Daily = 90,
        Weekly = 91,
        Monthly = 92,
        Yearly = 93,
        Minutely = 94,
        Secondly = 95
    }
}