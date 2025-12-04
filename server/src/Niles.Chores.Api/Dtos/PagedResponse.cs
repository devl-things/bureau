using System.Text.Json.Serialization;

namespace Niles.Chores.Api.Dtos
{
    public class PagedResponse<T>
    {
        [JsonPropertyName("data")]
        public IEnumerable<T> Data { get; set; } = new List<T>();

        [JsonPropertyName("meta")]
        public PagedMeta Meta { get; set; } = new PagedMeta();
    }

    public class PagedMeta
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

