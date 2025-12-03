using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Niles.Chores.Api.Utilities;
using Niles.Chores.Configurations;

namespace Niles.Chores.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // TODO! remake config script for deployment 
            // TODO SEEDER_USAGE.md should be updated because that was changed
            // TODO how can VS launch settings be included in git so that developers don't have to set them up manually and can I use that setting in rider 

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddChores(builder.Configuration.GetConnectionString("NilesDb")!);

            // Only register the CORS policy in development
            if (builder.Environment.IsDevelopment())
            {
                builder.Services.AddCors(options =>
                {
                    options.AddDefaultPolicy(policy =>
                    {
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
                });
            }

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddHealthChecks()
                .AddCheck<ChoresHealthCheck>("chores", failureStatus: HealthStatus.Unhealthy, tags: new[] { "ready" });

            WebApplication app = builder.Build();

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

                app.UseCors();
            }

            app.UseAuthorization();

            // Seed database in development
            if (app.Environment.IsDevelopment())
            {
                app.Services.SeedChores();
            }

            app.MapControllers();
            // TODO why are health checks not visible in swagger?
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
