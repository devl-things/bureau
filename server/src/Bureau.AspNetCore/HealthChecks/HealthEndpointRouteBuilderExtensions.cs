using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bureau.AspNetCore.HealthChecks
{
    public static class HealthEndpointRouteBuilderExtensions
    {
        public static IEndpointConventionBuilder MapBureauHealthChecksJson(this IEndpointRouteBuilder endpoints, string pattern)
        {
            HealthCheckOptions options = new HealthCheckOptions
            {
                ResponseWriter = WriteJsonAsync
            };

            return endpoints.MapHealthChecks(pattern, options);
        }

        public static IEndpointConventionBuilder MapBureauHealthChecksJson(this IEndpointRouteBuilder endpoints, string pattern, string tag)
        {
            HealthCheckOptions options = new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains(tag),
                ResponseWriter = WriteJsonAsync
            };

            return endpoints.MapHealthChecks(pattern, options);
        }

        private static async Task WriteJsonAsync(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";

            object response = new
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
    }
}
