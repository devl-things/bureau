using Microsoft.Extensions.Options;
using Niles.Chores.Web.Configurations;

namespace Niles.Chores.Web
{
    public class Program
    {
        public static void Main(string[] args)
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

            app.Run();
        }
    }
}
