using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Bureau.Server.Hosting.Dev
{
    public sealed class DevAutoSignInMiddleware
    {
        private readonly RequestDelegate _next;

        public DevAutoSignInMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context, IOptions<AuthOptions> authOptions, IDevPrincipalFactory principalFactory)
        {
            ArgumentNullException.ThrowIfNull(context);

            AuthOptions options = authOptions.Value;

            if (options.Mode != AuthMode.Dev || !options.Dev.AutoSignIn)
            {
                await _next(context);
                return;
            }

            string scopePath = options.Dev.ScopePath ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(scopePath))
            {
                PathString path = context.Request.Path;
                if (!path.StartsWithSegments(scopePath, StringComparison.OrdinalIgnoreCase))
                {
                    await _next(context);
                    return;
                }
            }

            if (context.User != null && context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                await _next(context);
                return;
            }

            ClaimsPrincipal principal = principalFactory.CreatePrincipal(options.Dev);
            await context.SignInAsync(AuthConstants.CookieScheme, principal);

            await _next(context);
        }
    }
}
