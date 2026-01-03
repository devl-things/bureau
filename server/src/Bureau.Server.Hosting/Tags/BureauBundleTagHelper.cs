using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace Bureau.Server.Hosting.Tags
{
    [HtmlTargetElement("bureau-bundle")]
    public sealed class BureauBundleTagHelper : TagHelper
    {
        private readonly IOptions<FrontendOptions> _options;

        public BureauBundleTagHelper(IOptions<FrontendOptions> options)
        {
            _options = options;
        }

        [HtmlAttributeName("name")]
        public string Name { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(output);

            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new InvalidOperationException("bureau-bundle requires a non-empty 'name' attribute.");
            }

            FrontendOptions options = _options.Value;

            output.TagName = null; // remove wrapper tag entirely
            output.TagMode = TagMode.StartTagAndEndTag;

            string html = options.UseViteDevServer ? BuildDevScripts(options, Name) : BuildProdScript(options, Name);

            output.Content.SetHtmlContent(html);
        }

        private static string BuildDevScripts(FrontendOptions options, string name)
        {
            if (!options.Bundles.TryGetValue(name, out string? entry) || string.IsNullOrWhiteSpace(entry))
            {
                throw new InvalidOperationException($"Frontend:Bundles is missing a dev entry for '{name}'.");
            }

            string origin = options.ViteDevServerOrigin.TrimEnd('/');
            string normalizedEntry = entry.StartsWith('/') ? entry : "/" + entry;

            string viteClient = origin + "/@vite/client";
            string reactPreamble = origin + "/src/react-refresh-preamble.ts";
            string entryUrl = origin + normalizedEntry;

            return $"""
<script type="module" src="{viteClient}"></script>
<script type="module" src="{reactPreamble}"></script>
<script type="module" src="{entryUrl}"></script>
""";
        }

        private static string BuildProdScript(FrontendOptions options, string name)
        {
            string publicPath = options.BundlesPublicPath.TrimEnd('/');
            string src = $"{publicPath}/{name}/{name}.js";

            return $"""<script type="module" src="{src}"></script>""";
        }
    }
}
