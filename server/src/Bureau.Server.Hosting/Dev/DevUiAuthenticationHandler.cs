using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace Bureau.Server.Hosting.Dev
{
    public sealed class DevUiAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IOptions<AuthOptions> _authOptions;
        private readonly IDevPrincipalFactory _principalFactory;

        public DevUiAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
            IOptions<AuthOptions> authOptions, IDevPrincipalFactory principalFactory)
            : base(options, logger, encoder)
        {
            _authOptions = authOptions ?? throw new ArgumentNullException(nameof(authOptions));
            _principalFactory = principalFactory ?? throw new ArgumentNullException(nameof(principalFactory));
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            AuthOptions options = _authOptions.Value;

            if (options.Mode != AuthMode.Dev)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            string scopePath = options.Dev.ScopePath ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(scopePath))
            {
                PathString path = Request.Path;
                if (!path.StartsWithSegments(scopePath, StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(AuthenticateResult.NoResult());
                }
            }

            System.Security.Claims.ClaimsPrincipal principal = _principalFactory.CreatePrincipal(options.Dev);
            AuthenticationTicket ticket = new AuthenticationTicket(principal, AuthConstants.DevUiScheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
