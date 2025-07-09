using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sven.Models;
using Sven.PageModels.Connect.ExternalProviders;
using Sven.Services;
using System.Security.Claims;

namespace Sven.PageModels.Connect.SignIn
{
    public abstract class SignInPageModel : PageModel
    {

        protected readonly ILogger<SignInPageModel> _logger;
        protected readonly IUserClaimsProvider _userClaimsProvider;
        public string Mode { get; set; } = PageModelTypes.SignIn.Plain;
        public string? ErrorMessage { get; set; }

        public ExternalProvidersViewModel ExternalProvidersViewModel { get; init; }

        protected SignInPageModel(ILogger<SignInPageModel> logger,
            ConnectTranslations translations,
            IUserClaimsProvider userProvider)
        {
            _logger = logger;
            _userClaimsProvider = userProvider;
            ExternalProvidersViewModel = new ExternalProvidersViewModel
            {
                Data = ExternalProviders.ExternalProviders.ExternalList,
                T9n = new ExternalProvidersTranslations(translations.SignInWith)
            };
        }

        public abstract Task<IActionResult> HandleGetRequestAsync(CancellationToken cancellationToken = default);

        public abstract Task<IActionResult> HandleLoginAsync(LoginCredentialsRequest credentials, CancellationToken cancellationToken = default);

        protected Task SignInUserAsync(ClaimsPrincipal userPrincipal, CancellationToken cancellationToken = default)
        {
            return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal);
        }
    }
}
