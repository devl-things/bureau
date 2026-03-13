using Bureau.Extensions.Logging;
using Bureau.Primitives.Health;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Bureau.AspNetCore.HealthChecks
{
    public sealed class ProbesHealthCheck : IHealthCheck
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ProbesHealthCheck> _logger;
        private readonly IReadOnlySet<string>? _tags;

        public ProbesHealthCheck(IServiceScopeFactory scopeFactory, ILogger<ProbesHealthCheck> logger, IReadOnlySet<string>? tags)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _tags = tags;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            using (IServiceScope scope = _scopeFactory.CreateScope())
            {
                List<IHealthProbe> probes = scope.ServiceProvider.GetServices<IHealthProbe>()
                    .Where(p => _tags == null || _tags.Count == 0 || p.Tags.Overlaps(_tags))
                    .ToList();

                if (probes.Count == 0)
                {
                    return HealthCheckResult.Unhealthy("No probes registered for the configured tags.");
                }
                ProbeRunSummary summary = await RunProbesAsync(probes, context.Registration.Name, cancellationToken);

                if (summary.HasFailures)
                {
                    return HealthCheckResult.Unhealthy("Unhealthy probes: " + summary.FailedKeysSummary, data: summary.Data);
                }

                return summary.HasData ? HealthCheckResult.Healthy(data: summary.Data) : HealthCheckResult.Healthy();
            }
        }

        private async Task<ProbeRunSummary> RunProbesAsync(IReadOnlyList<IHealthProbe> probes, string healthCheckName, CancellationToken cancellationToken)
        {
            ProbeRunSummary summary = new ProbeRunSummary();

            foreach (IHealthProbe probe in probes)
            {
                try
                {
                    Result<IReadOnlyDictionary<string, object>?> result = await probe.CheckAsync(cancellationToken);
                    if (result.IsSuccess)
                    {
                        if (result.Value != null && result.Value.Count > 0)
                        {
                            summary.Data[probe.Key] = result.Value;
                        }

                        continue;
                    }
                    summary.FailedKeys.Add(probe.Key);
                    _logger.LogResultError(result.Error, healthCheckName);

                    summary.Data[probe.Key] = CreateData(result.Error.Code, result.Error.ErrorMessage);
                }
                catch (Exception ex)
                {
                    summary.FailedKeys.Add(probe.Key);

                    _logger.LogError(ex, "Health probe '{ProbeKey}' threw an exception.", probe.Key);

                    summary.Data[probe.Key] = CreateData("probe_exception", "Probe failed with an unexpected exception.");
                }
            }

            return summary;
        }
        private sealed class ProbeRunSummary
        {
            public Dictionary<string, object> Data { get; } = new(StringComparer.OrdinalIgnoreCase);
            public List<string> FailedKeys { get; } = new();

            public string FailedKeysSummary => string.Join(", ", FailedKeys);
            public bool HasFailures => FailedKeys.Count > 0;
            public bool HasData => Data.Count > 0;

        }
        private static Dictionary<string, object> CreateData(string code, string message)
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["code"] = code,
                ["message"] = message
            };
        }
    }
}
