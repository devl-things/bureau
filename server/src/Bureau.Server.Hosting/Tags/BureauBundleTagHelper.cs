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

            string src = BuildScriptSrc(options, Name);

            output.TagName = "script";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("type", "module");
            output.Attributes.SetAttribute("src", src);
            output.Content.SetHtmlContent(string.Empty);
        }

        private static string BuildScriptSrc(FrontendOptions options, string name)
        {
            if (options.UseViteDevServer)
            {
                if (!options.Bundles.TryGetValue(name, out string? entry) || string.IsNullOrWhiteSpace(entry))
                {
                    throw new InvalidOperationException($"BureauFrontend:Bundles is missing a dev entry for '{name}'.");
                }

                string origin = options.ViteDevServerOrigin.TrimEnd('/');
                string normalizedEntry = entry.StartsWith('/') ? entry : "/" + entry;

                return origin + normalizedEntry;
            }

            string publicPath = options.BundlesPublicPath.TrimEnd('/');
            return $"{publicPath}/{name}.js";
        }
    }
}
