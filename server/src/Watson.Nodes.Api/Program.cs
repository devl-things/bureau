
using Bureau.AspNetCore.Cors;
using Bureau.AspNetCore.HealthChecks;
using Bureau.AspNetCore.Logging;
using Bureau.AspNetCore.Middleware;
using Bureau.AspNetCore.Serialization;
using Bureau.Primitives.Health;
using Bureau.Server.Hosting.Configurations;
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

            builder.Services.AddHealthChecks().AddProbes("nodes", ProbeTags.Database);

            WebApplication app = builder.Build();

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

            app.MapBureauHealthChecksJson(ApiRoutes.Health.Root);

            await app.RunAsync();
        }
    }
}
