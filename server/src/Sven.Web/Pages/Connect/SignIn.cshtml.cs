using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Sven.Configurations;
using Sven;
using Sven.PageModels.Connect;
using Sven.PageModels.Connect.ExternalProviders;
using Sven.PageModels.Connect.SignIn;
using Sven.Services;

namespace Sven.Pages.Connect
{
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("signin-endpoint")]
    public class SignInModel : PageModel
    {
        private readonly ISignInHandlerFactory _handlerFactory;

        public string Mode { get; private set; } = Modes.Connect.SignIn.Plain;
        public string? ErrorMessage { get; private set; }
        public ExternalProvidersViewModel ExternalProvidersViewModel { get; }
        public ISignInTranslations T9n { get; }

        public SignInModel(
            ConnectTranslations translations,
            ISignInHandlerFactory handlerFactory,
            IExternalProviderRegistry registry)
        {
            _handlerFactory = handlerFactory;
            T9n = translations;
            ExternalProvidersViewModel = new ExternalProvidersViewModel(registry)
            {
                T9n = new ExternalProvidersTranslations(translations.SignInWith)
            };
        }

        public Task<IActionResult> OnGetAsync([FromQuery(Name = AuthConstants.PropertyNames.Mode)] string? mode, CancellationToken cancellationToken = default)
        {
            SignInMode signInMode = ParseMode(mode);
            ISignInGetHandler handler = _handlerFactory.GetHandler(signInMode);
            SignInContext context = CreateContext(signInMode);
            return handler.HandleGetAsync(context, cancellationToken);
        }

        public async Task<IActionResult> OnPostLoginAsync([FromForm] LoginCredentialsRequest credentials, [FromForm(Name = AuthConstants.PropertyNames.Mode)] string? mode, CancellationToken cancellationToken = default)
        {
            SignInMode signInMode = ParseMode(mode);
            if (!_handlerFactory.TryGetPostHandler(signInMode, out ISignInPostHandler? handler))
            {
                return StatusCode(StatusCodes.Status405MethodNotAllowed);
            }

            SignInContext context = CreateContext(signInMode, credentials);
            IActionResult result = await handler.HandlePostAsync(context, cancellationToken);
            ErrorMessage = context.ErrorMessage;
            return result;
        }

        private static SignInMode ParseMode(string? mode)
        {
            return mode switch
            {
                Modes.Connect.SignIn.Pkce => SignInMode.Pkce,
                Modes.Connect.SignIn.Ticket => SignInMode.Ticket,
                _ => SignInMode.Plain
            };
        }

        private SignInContext CreateContext(SignInMode mode, LoginCredentialsRequest? credentials = null)
        {
            Mode = mode switch
            {
                SignInMode.Pkce => Modes.Connect.SignIn.Pkce,
                SignInMode.Ticket => Modes.Connect.SignIn.Ticket,
                _ => Modes.Connect.SignIn.Plain
            };
            ExternalProvidersViewModel.Data.SetMode(Mode);

            return new SignInContext(
                mode: mode,
                pageFactory: () => Page(),
                redirectFactory: (url) => Redirect(url),
                statusCodeFactory: (code) => StatusCode(code),
                request: Request,
                httpContext: HttpContext,
                credentials: credentials);
        }
    }
}
