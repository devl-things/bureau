using System.Text.Encodings.Web;
using System.Text.Json;

namespace Bureau.Server.Hosting
{
    public sealed class AppRuntimeOptions
    {
        public const string SectionName = "AppRuntime";
        public string Environment { get; set; } = "dev";
        public string Version { get; set; } = "local";
        public Dictionary<string, string> ApiBaseUrls { get; set; } = new Dictionary<string, string>();
        public string? DefaultApiKey { get; set; }
    }

    internal static class AppRuntimeJsonOptions
    {
        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.Default
        };
    }
}
