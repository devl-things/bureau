using Sven.Configurations;
using Sven.Services;

namespace Sven.Middleware
{
    public class CurrentUserMiddleware
    {
        private readonly RequestDelegate _next;

        public CurrentUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ICurrentUserProvider currentUser)
        {
            PathString path = context.Request.Path;

            if (path.StartsWithSegments(Endpoints.Account.AccountInfo))
            {
                await currentUser.SetUserAsync(context.User, context.RequestAborted);
            }
            await _next(context);
        }
    }

}
