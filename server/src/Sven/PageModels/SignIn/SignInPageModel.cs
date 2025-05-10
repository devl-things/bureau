using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Sven.Configurations;
using Sven.Models;
using Sven.Services;

namespace Sven.PageModels.SignIn
{
    public abstract class SignInPageModel : PageModel
    {
        private static readonly List<ExternalLoginSignInPageModel> _externalsList =
        [
            new ExternalLoginSignInPageModel()
            {
                IsSignInEnabled = true,
                SignInLink = Endpoints.External.SignInGoogle,
                ProviderName = "Google"
            },
            new ExternalLoginSignInPageModel()
            {
                IsSignInEnabled = false,
                SignInLink = Endpoints.External.SignInMicrosoft,
                ProviderName = "Microsoft"
            },
        ];
        protected readonly ILogger<SignInPageModel> _logger;
        protected readonly IUserClaimsProvider _userProvider;
        public string Mode { get; set; } = PageModelTypes.SignIn.Plain;
        public string? ErrorMessage { get; set; }
        public List<ExternalLoginSignInPageModel> ExternalLogins { get; set; }
        public string LoginEndpoint { get; set; } = Endpoints.Connect.Login;

        protected SignInPageModel(ILogger<SignInPageModel> logger, IUserClaimsProvider userProvider)
        {
            _logger = logger;
            _userProvider = userProvider;
            ExternalLogins = _externalsList;
        }

        public abstract IActionResult HandleGetRequest();

        public abstract Task<IActionResult> HandleLoginAsync(LoginCredentials credentials, CancellationToken cancellationToken = default);
    }
}
