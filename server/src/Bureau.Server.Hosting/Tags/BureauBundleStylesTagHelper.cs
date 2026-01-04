using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace Bureau.Server.Hosting.Tags
{
    [HtmlTargetElement("bureau-bundle-styles")]
    public sealed class BureauBundleStylesTagHelper : TagHelper
    {
        private readonly IOptions<FrontendOptions> _options;

        public BureauBundleStylesTagHelper(IOptions<FrontendOptions> options)
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
                throw new InvalidOperationException("bureau-bundle-styles requires a non-empty 'name' attribute.");
            }

            FrontendOptions options = _options.Value;

            output.TagName = null;
            output.TagMode = TagMode.StartTagAndEndTag;

            // Dev: Vite will inject CSS via JS imports + HMR; don't emit link tags.
            if (options.UseViteDevServer)
            {
                output.Content.SetHtmlContent(string.Empty);
                return;
            }

            string publicPath = options.BundlesPublicPath.TrimEnd('/');
            string href = $"{publicPath}/{Name}/{Name}.css";

            output.Content.SetHtmlContent($"""<link rel="stylesheet" href="{href}" />""");
        }
    }
}
