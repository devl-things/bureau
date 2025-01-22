using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.Identity.Configurations
{
    public static class BureauIdentityWebAppConfiguration
    {
        public static void UseBureauIdentity(this WebApplication app)
        {
            app.MapBureauIdentityServerAPIEndpoints();
            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}
