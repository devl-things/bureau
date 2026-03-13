using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Bureau.AspNetCore.Cors
{
    public static class CorsServiceCollectionExtensions
    {
        public static IServiceCollection AddBureauCors(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            CorsOptions corsOptions = new CorsOptions();

            configuration.GetSection(CorsOptions.SectionName).Bind(corsOptions);

            // Optional safety: disallow wildcard origins outside development
            if (!environment.IsDevelopment() && corsOptions.AllowedOrigins != null && corsOptions.AllowedOrigins.Any(origin => origin == "*"))
            {
                throw new InvalidOperationException("Wildcard CORS origins ('*') are not allowed outside development.");
            }

            services.AddCors(options =>
            {
                options.AddPolicy(CorsConstants.BureauCorsPolicyName, policy =>
                {
                    ConfigurePolicy(policy, corsOptions);
                });
            });

            return services;
        }

        private static void ConfigurePolicy(CorsPolicyBuilder policy, CorsOptions options)
        {
            if (options.AllowedOrigins != null && options.AllowedOrigins.Length > 0)
            {
                if (Array.Exists(options.AllowedOrigins, origin => origin == "*"))
                {
                    policy.SetIsOriginAllowed(IsLocalhostOrigin);
                }
                else
                {
                    policy.WithOrigins(options.AllowedOrigins);
                }
            }

            if (options.AllowedHeaders != null && options.AllowedHeaders.Length > 0)
            {
                if (Array.Exists(options.AllowedHeaders, header => header == "*"))
                {
                    policy.AllowAnyHeader();
                }
                else
                {
                    policy.WithHeaders(options.AllowedHeaders);
                }
            }

            if (options.AllowedMethods != null && options.AllowedMethods.Length > 0)
            {
                if (Array.Exists(options.AllowedMethods, method => method == "*"))
                {
                    policy.AllowAnyMethod();
                }
                else
                {
                    policy.WithMethods(options.AllowedMethods);
                }
            }
        }

        private static bool IsLocalhostOrigin(string origin)
        {
            if (string.IsNullOrWhiteSpace(origin))
            {
                return false;
            }

            if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri))
            {
                return false;
            }

            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(uri.Host, "::1", StringComparison.OrdinalIgnoreCase);
        }
    }
}
