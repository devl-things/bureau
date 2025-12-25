using Microsoft.AspNetCore.Builder;

namespace Bureau.AspNetCore.Cors
{
    public static class CorsApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseBureauCors(this IApplicationBuilder app)
        {
            app.UseCors(CorsConstants.DefaultCorsPolicyName);
            return app;
        }
    }
}
