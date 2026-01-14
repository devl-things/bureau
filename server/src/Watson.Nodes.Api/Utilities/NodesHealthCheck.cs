using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Watson.Nodes.Api.Utilities
{
    public class NodesHealthCheck : IHealthCheck
    {
        private readonly IChoresHealthService _healthService;

        public NodesHealthCheck(IChoresHealthService healthService)
        {
            _healthService = healthService;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (await _healthService.IsHealthyAsync(cancellationToken))
            {
                return HealthCheckResult.Healthy("Chores backend is healthy.");
            }

            return HealthCheckResult.Unhealthy("Chores backend cannot reach the database.");
        }
    }
}
