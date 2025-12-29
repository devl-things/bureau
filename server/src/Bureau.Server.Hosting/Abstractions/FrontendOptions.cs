namespace Bureau.Server.Hosting
{
    public sealed class FrontendOptions
    {
        public const string SectionName = "Frontend";

        public bool UseViteDevServer { get; set; }

        public string ViteDevServerOrigin { get; set; } = "http://localhost:5173";

        public string BundlesPublicPath { get; set; } = "/admin/assets";

        public Dictionary<string, string> Bundles { get; set; } = new Dictionary<string, string>();

    }
}
