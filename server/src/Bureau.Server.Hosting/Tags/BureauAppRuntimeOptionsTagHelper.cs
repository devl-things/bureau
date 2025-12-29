using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Bureau.Server.Hosting.Tags
{
    [HtmlTargetElement("bureau-app-runtime-options")]
    public sealed class BureauAppRuntimeOptionsTagHelper : TagHelper
    {
        private readonly IOptions<AppRuntimeOptions> _options;

        public BureauAppRuntimeOptionsTagHelper(IOptions<AppRuntimeOptions> options)
        {
            _options = options;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(output);

            AppRuntimeOptions value = _options.Value;

            string json = JsonSerializer.Serialize(value, AppRuntimeJsonOptions.Options);

            output.TagName = "script";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("type", "text/javascript");
            output.Content.SetHtmlContent($"window.__BUREAU__ = {json};");
        }
    }
}
