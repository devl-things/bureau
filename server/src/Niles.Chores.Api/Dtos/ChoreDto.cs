using System.Text.Json.Serialization;

namespace Niles.Chores.Api.Dtos
{
    public class ChoreDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        [JsonPropertyName("priority")]
        public int Priority { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("completed")]
        public bool IsCompleted { get; set; }
        [JsonPropertyName("repeatEveryWeeks")]
        public int? WeeklyInterval { get; set; }
        [JsonPropertyName("important")]
        public bool IsImportant { get; set; }
        [JsonPropertyName("importantReminderDate")]
        public DateTime? ImportantReminderDate { get; set; }
        [JsonPropertyName("importantNotes")]
        public string? ImportantNotes { get; set; }
        [JsonIgnore]
        public DateOnly Date { get; set; }
    }

}
