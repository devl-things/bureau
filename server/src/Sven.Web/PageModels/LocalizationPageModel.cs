using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;

namespace Sven.PageModels
{
    public abstract class LocalizationPageModel<TPageModel, TText> : PageModel
    {
        protected readonly IStringLocalizer<TPageModel> _pageLocalizer;
        protected readonly IStringLocalizer<Resources.Common> _localizer;
        public abstract TText Text { get; }
        protected LocalizationPageModel(IStringLocalizer<Resources.Common> localizer, IStringLocalizer<TPageModel> pageLocalizer)
        {
            _localizer = localizer;
            _pageLocalizer = pageLocalizer;
        }
    }
}
