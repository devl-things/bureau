using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bureau.AspNetCore.Serialization
{
    public static class BureauJsonSerializationServiceCollectionExtensions
    {
        public static IServiceCollection AddBureauJsonSerialization(
            this IServiceCollection services,
            Action<JsonOptions>? configure = null)
        {
            services.Configure<JsonOptions>(options =>
            {
                options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
                options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

                configure?.Invoke(options);
            });

            return services;
        }
    }
}
