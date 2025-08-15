namespace Niles.Etl.Lidl.Models
{
    public sealed class DownloaderJobArgs
    {
        public string? MainUrlOverride { get; init; }
        public string? BaseUrlOverride { get; init; }
        public string? TargetDirectory { get; init; }
        public bool Force { get; init; }

        public DownloaderJobArgs(
        string? mainUrlOverride = null,
        string? baseUrlOverride = null,
        string? targetDirectory = null,
        bool force = false)
        {
            MainUrlOverride = mainUrlOverride;
            BaseUrlOverride = baseUrlOverride;
            TargetDirectory = targetDirectory;
            Force = force;
        }

        public static DownloaderJobArgs From(Dictionary<string, string>? args)
        {
            if (args is null || args.Count == 0)
            {
                return new DownloaderJobArgs();
            }

            args = new Dictionary<string, string>(args, StringComparer.OrdinalIgnoreCase);
            //TODO: magic strings
            args.TryGetValue("mainUrl", out string? main);
            args.TryGetValue("baseUrl", out string? baseUrl);
            args.TryGetValue("targetDir", out string? target);

            bool force = false;
            if (args.TryGetValue("force", out string? f))
            {
                _ = bool.TryParse(f, out force);
            }

            return new DownloaderJobArgs(
                mainUrlOverride: string.IsNullOrWhiteSpace(main) ? null : main,
                baseUrlOverride: string.IsNullOrWhiteSpace(baseUrl) ? null : baseUrl,
                targetDirectory: string.IsNullOrWhiteSpace(target) ? null : target,
                force: force
            );
        }
    }
}
