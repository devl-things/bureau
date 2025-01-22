using System.Text.Json.Serialization;

namespace Bureau.Google.Calendar.Data
{
    internal class CalendarNotification
    {
        /// <summary>
        /// The method used to deliver the notification. The possible value is:   - "email" - Notifications are sent via
        /// email.   Required when adding a notification.
        /// </summary>
        [JsonPropertyName("method")]
        public virtual string Method { get; set; }

        /// <summary>
        /// The type of notification. Possible values are:   - "eventCreation" - Notification sent when a new event is
        /// put on the calendar.  - "eventChange" - Notification sent when an event is changed.  - "eventCancellation" -
        /// Notification sent when an event is cancelled.  - "eventResponse" - Notification sent when an attendee
        /// responds to the event invitation.  - "agenda" - An agenda with the events of the day (sent out in the
        /// morning).   Required when adding a notification.
        /// </summary>
        [JsonPropertyName("type")]
        public virtual string Type { get; set; }

        /// <summary>The ETag of the item.</summary>
        public virtual string ETag { get; set; }
    }
}
