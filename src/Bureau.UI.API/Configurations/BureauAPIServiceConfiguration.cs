using Asp.Versioning;
using Bureau.UI.API.OpenApi;
using Bureau.UI.API.V1.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Bureau.UI.API.Configurations
{
    public static class BureauAPIServiceConfiguration
    {
        public static void AddBureauAPI(this IServiceCollection services)
        {
            // open api
            services.AddEndpointsApiExplorer();
            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen(options => options.OperationFilter<SwaggerDefaultValues>());

            // we remove null values from the response
            services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
            {
                options.SerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            });

            // api versioning
            services.AddApiVersioning(options => 
            {
                options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new HeaderApiVersionReader("api-version");
            })
                .AddApiExplorer(options => 
                {
                    options.GroupNameFormat = "'v'VVV";
                });

            //services.AddProblemDetails();

            services.AddRecipes();
        }
    }
}
