using Niles.Chores.Abstractions.Services;
using Niles.Chores.Services;

namespace Niles.Chores.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddSingleton<IHousekeepingService, HousekeepingService>();
            builder.Services.AddSingleton<IPrioritizedChoreService, PrioritizedChoreService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
            }

            app.UseAuthorization();

            app.MapControllers();
            app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

            app.Run();
        }
    }
}
