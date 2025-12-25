using Bureau.Admin.Hosting.Configurations;
using Bureau.Server.Hosting.Configurations;
using Microsoft.Extensions.Options;
using Niles.Chores.Web.Configurations;

namespace Niles.Chores.Web
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseStaticWebAssets();

            builder.Services.Configure<AppConfiguration>(builder.Configuration);

            builder.Services.AddBureauAdminAppsRegistry(builder.Configuration);
            builder.Services.AddRazorPages();

            builder.Services.AddBureauUiAuth(builder.Configuration);

            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapGet("/config", (IOptions<AppConfiguration> config) =>
            {
                return Results.Json(config.Value);
            });

            await app.RunAsync();
        }
    }
}
