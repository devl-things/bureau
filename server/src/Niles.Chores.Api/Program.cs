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
            builder.Services.AddSingleton<IChoreService, ChoreService>();

            // Add CORS support
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
            }

            // Use CORS before authorization
            app.UseCors();

            app.UseAuthorization();

            app.MapControllers();
            app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));

            app.Run();
        }
    }
}
