using Bureau.Primitives;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Bureau.AspNetCore.HealthChecks
{
    public sealed class ProbeHealthCheck<TProbe> : IHealthCheck where TProbe : class, IHealthProbe
    {
        private readonly TProbe _probe;

        public ProbeHealthCheck(TProbe probe)
        {
            _probe = probe;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            return await _probe.IsHealthyAsync(cancellationToken) ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        }
    }
}
