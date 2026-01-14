
using Bureau.AspNetCore.Cors;
using Bureau.AspNetCore.Logging;
using Bureau.AspNetCore.Middleware;
using Bureau.AspNetCore.Serialization;
using Bureau.Server.Hosting.Configurations;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Watson.Nodes.Contracts;
using Watson.Nodes.Data.SqlServer.Configurations;

namespace Watson.Nodes.Api
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Logging.AddBureauActivityTracking();
            builder.Services.AddBureauJsonSerialization();

            builder.Services.AddWatsonNodesSqlServer(builder.Configuration);

            builder.Services.AddBureauCors(builder.Configuration, builder.Environment);
            builder.Services.AddBureauApiAuth(builder.Configuration);

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //TODO health checks
            builder.Services.AddHealthChecks()
                .AddCheck<ChoresHealthCheck>("chores", failureStatus: HealthStatus.Unhealthy, tags: new[] { "ready" });


            var app = builder.Build();

            app.Services.MigrateWatsonNodes();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Watson.Nodes v1");
                    options.RoutePrefix = "swagger";
                });
            }

            app.UseMiddleware<ApiExceptionHandlingMiddleware>();

            app.UseBureauCors();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapHealthChecks(ApiRoutes.Health.Root,
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

            await app.RunAsync();
        }
    }
}
