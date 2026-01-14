using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Bureau.AspNetCore.HealthChecks
{
    public static class HealthChecksBuilderExtensions
    {
        /// <summary>
        /// Registers a health check that executes all IHealthProbe instances (optionally filtered by a tag).
        /// This creates a SEPARATE health check entry per call (separation by tag).
        /// </summary>
        public static IHealthChecksBuilder AddProbes(this IHealthChecksBuilder builder, string name, string? tag = null, IEnumerable<string>? healthCheckTags = null)
        {
            HealthCheckRegistration registration = new HealthCheckRegistration(name,
                sp =>
                {
                    IServiceScopeFactory scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                    ILogger<ProbesHealthCheck> logger = sp.GetRequiredService<ILogger<ProbesHealthCheck>>();
                    return new ProbesHealthCheck(scopeFactory, logger, tag);
                },
                failureStatus: null,
                tags: healthCheckTags?.ToArray());
            return builder.Add(registration);
        }
    }
}
