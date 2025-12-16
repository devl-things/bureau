using System.Text.Json.Serialization;

namespace Bureau.Server.Contracts
{
    public class BureauPagedResponse<T>
    {
        [JsonPropertyName("data")]
        public IEnumerable<T> Data { get; set; } = Array.Empty<T>();

        [JsonPropertyName("meta")]
        public BureauPagedMeta Meta { get; set; } = new BureauPagedMeta();
    }

    public class BureauPagedMeta
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }

        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }

        [JsonPropertyName("total")]
        public int Total { get; set; }

        [JsonPropertyName("totalPages")]
        public int TotalPages { get; set; }

        [JsonPropertyName("hasNext")]
        public bool HasNext { get; set; }

        [JsonPropertyName("hasPrevious")]
        public bool HasPrevious { get; set; }
    }
}
