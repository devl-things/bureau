using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;
using Sven.Models;
using Sven.Services;
using System.Security.Claims;

namespace Sven.PageModels.SignIn
{
    public abstract class SignInPageModel : PageModel
    {

        protected readonly ILogger<SignInPageModel> _logger;
        protected readonly IUserClaimsProvider _userClaimsProvider;
        public string Mode { get; set; } = PageModelTypes.SignIn.Plain;
        public string? ErrorMessage { get; set; }
        public List<ExternalLoginPageModel> ExternalLogins { get; set; }

        protected SignInPageModel(ILogger<SignInPageModel> logger, IUserClaimsProvider userProvider)
        {
            _logger = logger;
            _userClaimsProvider = userProvider;
            ExternalLogins = ExternalLoginProviders.ExternalList;
        }

        public abstract Task<IActionResult> HandleGetRequestAsync(CancellationToken cancellationToken = default);

        public abstract Task<IActionResult> HandleLoginAsync(LoginCredentials credentials, CancellationToken cancellationToken = default);

        protected Task SignInUserAsync(ClaimsPrincipal userPrincipal, CancellationToken cancellationToken = default)
        {
            return HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, userPrincipal);
        }

        protected string? GetQueryStringParameter(string parameterName)
        {
            return Request.Query.TryGetValue(parameterName, out StringValues values) ? values.FirstOrDefault() : null;
        }
    }
}
