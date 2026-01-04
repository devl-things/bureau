using Bureau.Server.Hosting;
using Microsoft.Extensions.Options;
using Niles.Chores.Contracts;

namespace Niles.Chores.Web.Configurations
{
    public sealed class ChoresRuntimeOptionsConfigurator : IPostConfigureOptions<AppRuntimeOptions>
    {
        public void PostConfigure(string? name, AppRuntimeOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            if (string.IsNullOrWhiteSpace(options.DefaultApiKey))
            {
                return;
            }

            if (!options.ApiBaseUrls.TryGetValue(options.DefaultApiKey, out string? baseUrl))
            {
                return;
            }

            string normalizedBaseUrl = NormalizeBaseUrl(baseUrl);

            // Optional: default key for “main API”
            options.DefaultApiKey ??= "choresApiBase";
            // Base
            options.ApiBaseUrls[options.DefaultApiKey] = normalizedBaseUrl;

            // Derived absolute endpoints (templates)
            options.ApiBaseUrls["chores.chores"] = Combine(normalizedBaseUrl, ApiRoutes.Chores.Root);
            options.ApiBaseUrls["chores.choresById"] = Combine(normalizedBaseUrl, ApiRoutes.Chores.ByIdUrl);
            options.ApiBaseUrls["chores.critical"] = Combine(normalizedBaseUrl, ApiRoutes.Chores.CriticalUrl);

            options.ApiBaseUrls["chores.housekeeping"] = Combine(normalizedBaseUrl, ApiRoutes.Housekeeping.Root);
            options.ApiBaseUrls["chores.prioritizedChores"] = Combine(normalizedBaseUrl, ApiRoutes.Housekeeping.PrioritizedChoresUrl);
            options.ApiBaseUrls["chores.housekeepingById"] = Combine(normalizedBaseUrl, ApiRoutes.Housekeeping.ByIdUrl);
            options.ApiBaseUrls["chores.housekeepingSubmit"] = Combine(normalizedBaseUrl, ApiRoutes.Housekeeping.SubmitUrl);

            options.ApiBaseUrls["chores.health"] = Combine(normalizedBaseUrl, ApiRoutes.Health.Root);

        }

        private static string NormalizeBaseUrl(string value)
        {
            string trimmed = value.Trim();

            if (trimmed.EndsWith('/'))
            {
                trimmed = trimmed.TrimEnd('/');
            }

            return trimmed;
        }

        private static string Combine(string baseUrl, string routeTemplate)
        {
            string path = routeTemplate.TrimStart('/');

            return $"{baseUrl}/{path}";
        }
    }
}
