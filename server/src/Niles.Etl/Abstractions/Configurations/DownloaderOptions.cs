namespace Niles.Etl.Abstractions.Configurations
{
    public class DownloaderOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string MainUrl { get; set; } = string.Empty;
        public string TempDirectory { get; set; } = string.Empty;

    }
}
