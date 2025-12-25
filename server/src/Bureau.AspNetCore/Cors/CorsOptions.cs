namespace Bureau.AspNetCore.Cors
{
    public sealed class CorsOptions
    {
        public const string SectionName = "Cors";
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
        public string[] AllowedHeaders { get; set; } = Array.Empty<string>();
        public string[] AllowedMethods { get; set; } = Array.Empty<string>();
    }
}
