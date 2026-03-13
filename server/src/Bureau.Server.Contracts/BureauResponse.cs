using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Bureau.Server.Contracts
{
    public sealed class BureauResponse<T>
    {
        [JsonPropertyName("data")]
        public required T Data { get; init; }

        [SetsRequiredMembers]
        public BureauResponse(T data)
        {
            Data = data;
        }
    }
}
