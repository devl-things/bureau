using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Niles.Chores.Api.Utilities;
using Niles.Chores.Configurations;

namespace Niles.Chores.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddChores(builder.Configuration.GetConnectionString("NilesDb")!);

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddHealthChecks()
                .AddCheck<ChoresHealthCheck>("chores", failureStatus: HealthStatus.Unhealthy, tags: new[] { "ready" });

            var app = builder.Build();

            app.Services.MigrateChores();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Housekeeping API v1");
                    options.RoutePrefix = "swagger";
                });
            }

            app.UseAuthorization();


            app.MapControllers();
            app.MapHealthChecks("api/health",
                new HealthCheckOptions
                {
                    ResponseWriter = async (context, report) =>
                    {
                        context.Response.ContentType = "application/json";

                        var response = new
                        {
                            status = report.Status.ToString(),
                            entries = report.Entries.Select(x => new
                            {
                                key = x.Key,
                                status = x.Value.Status.ToString(),
                                description = x.Value.Description
                            })
                        };

                        await context.Response.WriteAsJsonAsync(response);
                    }
                });

            app.Run();
        }
    }
}
