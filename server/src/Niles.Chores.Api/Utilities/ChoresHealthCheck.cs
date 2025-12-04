using Microsoft.Extensions.Diagnostics.HealthChecks;
using Niles.Chores.Services;

namespace Niles.Chores.Api.Utilities
{
    public class ChoresHealthCheck : IHealthCheck
    {
        private readonly IChoresHealthService _healthService;

        public ChoresHealthCheck(IChoresHealthService healthService)
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
