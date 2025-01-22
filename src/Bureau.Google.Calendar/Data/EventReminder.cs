using System.Text.Json.Serialization;

namespace Bureau.Google.Calendar.Data
{
    internal class EventReminder
    {
        /// <summary>
        /// The method used by this reminder. Possible values are:   - "email" - Reminders are sent via email.  -
        /// "popup" - Reminders are sent via a UI popup.   Required when adding a reminder.
        /// </summary>
        [JsonPropertyName("method")]
        public virtual string Method { get; set; }

        /// <summary>
        /// Number of minutes before the start of the event when the reminder should trigger. Valid values are between 0
        /// and 40320 (4 weeks in minutes). Required when adding a reminder.
        /// </summary>
        [JsonPropertyName("minutes")]
        public virtual System.Nullable<int> Minutes { get; set; }

        /// <summary>The ETag of the item.</summary>
        public virtual string ETag { get; set; }
    }
}
