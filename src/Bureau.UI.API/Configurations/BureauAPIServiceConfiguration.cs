using Asp.Versioning;
using Bureau.Calendar.Configurations;
using Bureau.Configurations;
using Bureau.Recipes.Configurations;
using Bureau.UI.API.OpenApi;
using Bureau.UI.API.Services;
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
                options.DefaultApiVersion = BureauAPIVersion.Version1;
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionReader = new HeaderApiVersionReader("api-version");

                // TIPS other chatgpt suggestions
                //  X-API-Version: Use if your organization prefers explicit custom headers.
                //  API-Version: Use for a cleaner, modern approach.
                //  Accept-Version: Use if you want to align versioning with content negotiation.
                //  Version: Use for simplicity in small or internal APIs.
            })
                .AddApiExplorer(options =>
                {
                    options.GroupNameFormat = "'v'VVV";
                });

            //services.AddProblemDetails();

            services.AddHttpContextAccessor();
            services.AddScoped<BureauLinkGenerator>();

            services.AddBureauCommon();

            services.AddBureauRecipes();

            services.AddBureauCalendar();
        }
    }
}
