using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sven.Configurations;
using Sven.Models;
using Sven.Services;

namespace Sven.PageModels.SignIn
{
    public abstract class SignInPageModel : PageModel
    {

        protected readonly ILogger<SignInPageModel> _logger;
        protected readonly IUserClaimsProvider _userProvider;
        public string Mode { get; set; } = PageModelTypes.SignIn.Plain;
        public string? ErrorMessage { get; set; }
        public List<ExternalLoginPageModel> ExternalLogins { get; set; }
        public string LoginEndpoint { get; set; } = Endpoints.Connect.Login;

        protected SignInPageModel(ILogger<SignInPageModel> logger, IUserClaimsProvider userProvider)
        {
            _logger = logger;
            _userProvider = userProvider;
            ExternalLogins = ExternalLoginProviders.ExternalList;
        }

        public abstract IActionResult HandleGetRequest();

        public abstract Task<IActionResult> HandleLoginAsync(LoginCredentials credentials, CancellationToken cancellationToken = default);
    }
}
