using System.Text.Json.Serialization;

namespace Bureau.Server.Contracts
{
    public sealed class BureauCursorResponse<T>
    {
        [JsonPropertyName("data")]
        public required IReadOnlyList<T> Data { get; init; }

        [JsonPropertyName("meta")]
        public required BureauCursorMeta Meta { get; init; }
    }

    public sealed class BureauCursorMeta
    {
        // Cursor used/acknowledged by the server for this response (optional but useful).
        // If client didn't provide cursor, you can return 0 or null. Here kept nullable to be honest.
        [JsonPropertyName("cursor")]
        public long? Cursor { get; init; }

        [JsonPropertyName("nextCursor")]
        public required long NextCursor { get; init; }

        [JsonPropertyName("hasMore")]
        public required bool HasMore { get; init; }

        [JsonPropertyName("count")]
        public required int Count { get; init; }

        // Optional extra info for feeds like /changes (compact/full)
        [JsonPropertyName("mode")]
        public string? Mode { get; init; }

        // Optional continuation token for non-numeric cursors (e.g. search "next")
        // Keep it nullable; only set it for endpoints that need it.
        [JsonPropertyName("next")]
        public string? Next { get; init; }
    }
}
