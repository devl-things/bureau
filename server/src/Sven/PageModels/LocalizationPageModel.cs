using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Sven.PageModels
{
    public abstract class LocalizationPageModel<TPageModel, TText> : PageModel
    {
        protected readonly IStringLocalizer<TPageModel> _localizer;
        protected readonly IStringLocalizer<Common> _sharedLocalizer;
        public abstract TText Text { get; }
        protected LocalizationPageModel(IStringLocalizer<Common> sharedLocalizer, IStringLocalizer<TPageModel> localizer)
        {
            _sharedLocalizer = sharedLocalizer;
            _localizer = localizer;
        }
    }
}
