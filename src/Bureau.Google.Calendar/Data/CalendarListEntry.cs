using System.Text.Json.Serialization;

namespace Bureau.Google.Calendar.Data
{
    internal class CalendarListEntry
    {
        /// <summary>
        /// The effective access role that the authenticated user has on the calendar. Read-only. Possible values are:
        /// - "freeBusyReader" - Provides read access to free/busy information.  - "reader" - Provides read access to
        /// the calendar. Private events will appear to users with reader access, but event details will be hidden.  -
        /// "writer" - Provides read and write access to the calendar. Private events will appear to users with writer
        /// access, and event details will be visible.  - "owner" - Provides ownership of the calendar. This role has
        /// all of the permissions of the writer role with the additional ability to see and manipulate ACLs.
        /// </summary>
        [JsonPropertyName("accessRole")]
        public virtual string AccessRole { get; set; }

        /// <summary>
        /// The main color of the calendar in the hexadecimal format "#0088aa". This property supersedes the index-based
        /// colorId property. To set or change this property, you need to specify colorRgbFormat=true in the parameters
        /// of the insert, update and patch methods. Optional.
        /// </summary>
        [JsonPropertyName("backgroundColor")]
        public virtual string BackgroundColor { get; set; }

        /// <summary>
        /// The color of the calendar. This is an ID referring to an entry in the calendar section of the colors
        /// definition (see the colors endpoint). This property is superseded by the backgroundColor and foregroundColor
        /// properties and can be ignored when using these properties. Optional.
        /// </summary>
        [JsonPropertyName("colorId")]
        public virtual string ColorId { get; set; }

        /// <summary>
        /// Conferencing properties for this calendar, for example what types of conferences are allowed.
        /// </summary>
        [JsonPropertyName("conferenceProperties")]
        public virtual ConferenceProperties ConferenceProperties { get; set; }

        /// <summary>The default reminders that the authenticated user has for this calendar.</summary>
        [JsonPropertyName("defaultReminders")]
        public virtual System.Collections.Generic.IList<EventReminder> DefaultReminders { get; set; }

        /// <summary>
        /// Whether this calendar list entry has been deleted from the calendar list. Read-only. Optional. The default
        /// is False.
        /// </summary>
        [JsonPropertyName("deleted")]
        public virtual System.Nullable<bool> Deleted { get; set; }

        /// <summary>Description of the calendar. Optional. Read-only.</summary>
        [JsonPropertyName("description")]
        public virtual string Description { get; set; }

        /// <summary>ETag of the resource.</summary>
        [JsonPropertyName("etag")]
        public virtual string ETag { get; set; }

        /// <summary>
        /// The foreground color of the calendar in the hexadecimal format "#ffffff". This property supersedes the
        /// index-based colorId property. To set or change this property, you need to specify colorRgbFormat=true in the
        /// parameters of the insert, update and patch methods. Optional.
        /// </summary>
        [JsonPropertyName("foregroundColor")]
        public virtual string ForegroundColor { get; set; }

        /// <summary>
        /// Whether the calendar has been hidden from the list. Optional. The attribute is only returned when the
        /// calendar is hidden, in which case the value is true.
        /// </summary>
        [JsonPropertyName("hidden")]
        public virtual System.Nullable<bool> Hidden { get; set; }

        /// <summary>Identifier of the calendar.</summary>
        [JsonPropertyName("id")]
        public virtual string Id { get; set; }

        /// <summary>Type of the resource ("calendar#calendarListEntry").</summary>
        [JsonPropertyName("kind")]
        public virtual string Kind { get; set; }

        /// <summary>Geographic location of the calendar as free-form text. Optional. Read-only.</summary>
        [JsonPropertyName("location")]
        public virtual string Location { get; set; }

        /// <summary>The notifications that the authenticated user is receiving for this calendar.</summary>
        [JsonPropertyName("notificationSettings")]
        public virtual NotificationSettingsData NotificationSettings { get; set; }

        /// <summary>
        /// Whether the calendar is the primary calendar of the authenticated user. Read-only. Optional. The default is
        /// False.
        /// </summary>
        [JsonPropertyName("primary")]
        public virtual System.Nullable<bool> Primary { get; set; }

        /// <summary>Whether the calendar content shows up in the calendar UI. Optional. The default is False.</summary>
        [JsonPropertyName("selected")]
        public virtual System.Nullable<bool> Selected { get; set; }

        /// <summary>Title of the calendar. Read-only.</summary>
        [JsonPropertyName("summary")]
        public virtual string Summary { get; set; }

        /// <summary>The summary that the authenticated user has set for this calendar. Optional.</summary>
        [JsonPropertyName("summaryOverride")]
        public virtual string SummaryOverride { get; set; }

        /// <summary>The time zone of the calendar. Optional. Read-only.</summary>
        [JsonPropertyName("timeZone")]
        public virtual string TimeZone { get; set; }

        /// <summary>The notifications that the authenticated user is receiving for this calendar.</summary>
        public class NotificationSettingsData
        {
            /// <summary>The list of notifications set for this calendar.</summary>
            [JsonPropertyName("notifications")]
            public virtual System.Collections.Generic.IList<CalendarNotification> Notifications { get; set; }
        }
    }
}
