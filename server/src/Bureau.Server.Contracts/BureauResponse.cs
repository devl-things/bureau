using System.Text.Json.Serialization;

namespace Bureau.Server.Contracts
{
    public sealed class BureauResponse<T>
    {
        [JsonPropertyName("data")]
        public required T Data { get; init; }

        public BureauResponse(T data)
        {
            Data = data;
        }
    }
}
