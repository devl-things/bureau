using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sven.Configurations;
using Sven.Models;
using Sven.PageModels.Connect.ExternalProviders;
using Sven.Services;
using System.Security.Claims;

namespace Sven.PageModels.Connect.SignIn
{
    public abstract class SignInPageModel : PageModel
    {
        protected string _mode;
        protected readonly ILogger<SignInPageModel> _logger;
        protected readonly IUserClaimsProvider _userClaimsProvider;
        public string Mode { get { return _mode; } }
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
                T9n = new ExternalProvidersTranslations(translations.SignInWith)
            };
        }
        protected void SetMode(string? mode)
        {
            _mode = string.IsNullOrWhiteSpace(mode) ? Modes.Connect.SignIn.Plain : mode;
            ExternalProvidersViewModel.Data.SetMode(_mode);
        }

        public abstract Task<IActionResult> HandleGetRequestAsync(CancellationToken cancellationToken = default);

        public abstract Task<IActionResult> HandleLoginAsync(LoginCredentialsRequest credentials, CancellationToken cancellationToken = default);

        protected Task SignInUserAsync(ClaimsPrincipal userPrincipal, CancellationToken cancellationToken = default)
        {
            return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal);
        }
    }
}
