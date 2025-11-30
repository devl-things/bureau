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
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddChores(builder.Configuration.GetConnectionString("NilesDb")!);

            // Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

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

            app.UseCors();
            app.UseAuthorization();

            // Seed database in development
            if (app.Environment.IsDevelopment())
            {
                using (IServiceScope scope = app.Services.CreateScope())
                {
                    try
                    {
                        // Get ChoresContext using reflection since it's internal
                        Type? choresContextType = typeof(Niles.Chores.Configurations.IServiceCollectionExtension)
                            .Assembly
                            .GetType("Niles.Chores.Contexts.ChoresContext");
                        
                        if (choresContextType != null)
                        {
                            Microsoft.EntityFrameworkCore.DbContext? context = scope.ServiceProvider.GetRequiredService(choresContextType) as Microsoft.EntityFrameworkCore.DbContext;
                            if (context != null)
                            {
                                await Niles.Chores.Data.ChoresSeeder.SeedAsync(context);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred while seeding the database.");
                    }
                }
            }

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
