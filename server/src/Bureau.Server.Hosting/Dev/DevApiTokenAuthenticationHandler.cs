using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Bureau.Server.Hosting.Dev
{
    public sealed class DevApiTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IOptions<AuthOptions> _authOptions;
        private readonly IDevPrincipalFactory _principalFactory;

        public DevApiTokenAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder,
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

            string? header = Request.Headers.Authorization;

            if (string.IsNullOrWhiteSpace(header))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            const string bearerPrefix = "Bearer ";
            if (!header.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            string token = header.Substring(bearerPrefix.Length).Trim();

            if (!string.Equals(token, options.Dev.ApiToken, StringComparison.Ordinal))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid dev token."));
            }

            ClaimsPrincipal principal = _principalFactory.CreatePrincipal(options.Dev);
            AuthenticationTicket ticket = new AuthenticationTicket(principal, AuthConstants.DevApiTokenScheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
