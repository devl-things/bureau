using Bureau.Primitives;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.AspNetCore.HealthChecks
{
    public static class HealthChecksBuilderExtensions
    {
        public static IHealthChecksBuilder AddProbe<TProbe>(this IHealthChecksBuilder builder, string name) where TProbe : class, IHealthProbe
        {
            builder.Services.AddSingleton<TProbe>();
            return builder.AddCheck<ProbeHealthCheck<TProbe>>(name);
        }
    }
}
