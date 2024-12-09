using System.Text.Json.Serialization;

namespace Bureau.Google.Calendar.Data
{
    internal class CalendarList
    {
        /// <summary>ETag of the collection.</summary>
        [JsonPropertyName("etag")]
        public virtual string ETag { get; set; }

        /// <summary>Calendars that are present on the user's calendar list.</summary>
        [JsonPropertyName("items")]
        public virtual System.Collections.Generic.IList<CalendarListEntry> Items { get; set; }

        /// <summary>Type of the collection ("calendar#calendarList").</summary>
        [JsonPropertyName("kind")]
        public virtual string Kind { get; set; }

        /// <summary>
        /// Token used to access the next page of this result. Omitted if no further results are available, in which
        /// case nextSyncToken is provided.
        /// </summary>
        [JsonPropertyName("nextPageToken")]
        public virtual string NextPageToken { get; set; }

        /// <summary>
        /// Token used at a later point in time to retrieve only the entries that have changed since this result was
        /// returned. Omitted if further results are available, in which case nextPageToken is provided.
        /// </summary>
        [JsonPropertyName("nextSyncToken")]
        public virtual string NextSyncToken { get; set; }
    }
}
