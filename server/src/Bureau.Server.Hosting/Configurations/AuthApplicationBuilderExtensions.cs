using Bureau.Server.Hosting.Dev;
using Microsoft.AspNetCore.Builder;

namespace Bureau.Server.Hosting.Configurations
{
    public static class AuthApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseBureauDevAutoSignIn(this IApplicationBuilder app)
        {
            return app.UseMiddleware<DevAutoSignInMiddleware>();
        }
    }
}
