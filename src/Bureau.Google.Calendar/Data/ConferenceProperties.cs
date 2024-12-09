using System.Text.Json.Serialization;

namespace Bureau.Google.Calendar.Data
{
    internal class ConferenceProperties
    {
        /// <summary>
        /// The types of conference solutions that are supported for this calendar. The possible values are:   -
        /// "eventHangout"  - "eventNamedHangout"  - "hangoutsMeet"  Optional.
        /// </summary>
        [JsonPropertyName("allowedConferenceSolutionTypes")]
        public virtual System.Collections.Generic.IList<string> AllowedConferenceSolutionTypes { get; set; }

        /// <summary>The ETag of the item.</summary>
        public virtual string ETag { get; set; }
    }
}
