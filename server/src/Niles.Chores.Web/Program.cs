using Microsoft.Extensions.Options;
using Niles.Chores.Web.Configurations;

namespace Niles.Chores.Web
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<AppConfiguration>(builder.Configuration);


            var app = builder.Build();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.MapGet("/config", (IOptions<AppConfiguration> config) =>
            {
                return Results.Json(config.Value);
            });

            await app.RunAsync();
        }
    }
}
