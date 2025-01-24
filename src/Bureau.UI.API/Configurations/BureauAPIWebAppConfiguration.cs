using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Builder;
using Bureau.UI.API.Features.Calendar.Configurations;
using Bureau.UI.API.Features.Recipes.Configurations;
using Bureau.UI.API.Features.Recipes.V1.Configurations;
using Bureau.UI.API.Features.Recipes.V2.Configurations;
using Bureau.UI.API.Features.Recipes.V3.Configurations;
using Bureau.UI.API.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;

namespace Bureau.UI.API.Configurations
{
    public static class BureauAPIWebAppConfiguration
    {
        public static void MapBureauAPI(this WebApplication app)
        {
            ApiVersionSet versionSet1 = app.NewApiVersionSet()
                .HasApiVersion(BureauAPIVersion.Version1)
                .ReportApiVersions()
                .Build();
            ApiVersionSet versionSet3 = app.NewApiVersionSet()
                .HasApiVersion(BureauAPIVersion.Version1)
                .HasApiVersion(BureauAPIVersion.Version2)
                .HasApiVersion(BureauAPIVersion.Version3)
                .ReportApiVersions()
                .Build();

            app.Urls.Add("http://0.0.0.0:5580");

            RouteGroupBuilder apiBuilder = app.MapGroup("api");

            apiBuilder.MapRecipes(versionSet3);

            apiBuilder.MapCalendar(versionSet1);

            app.MapBureauOpenApiExplorer();

            //app.UseExceptionManager();
        }

        internal static void MapBureauOpenApiExplorer(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    IReadOnlyList<ApiVersionDescription> descriptions = app.DescribeApiVersions();
                    foreach (ApiVersionDescription description in descriptions)
                    {
                        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                    }
                });
            }
        }

        internal static void UseExceptionManager(this WebApplication app)
        {
            app.UseExceptionHandler(errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

                    context.Response.ContentType = "application/json";

                    if (exception is BadHttpRequestException badRequestException)
                    {
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsJsonAsync(new ApiResponse()
                        {
                            Status = "error",
                            Message = "Bad request",
                        });
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        await context.Response.WriteAsJsonAsync(new ApiResponse()
                        {
                            Status = "error",
                            Message = "An unexpected error occurred",
                        });
                    }
                });
            });
        }
    }
}
