namespace Bureau.Primitives.Health
{
    public interface IHealthProbe
    {
        /// <summary>
        /// Stable, unique key identifying this probe within the service.
        /// Example: "db-main", "db-reporting", "redis".
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Tags used for grouping probes into multiple health endpoints/check registrations.
        /// Example: "db", "critical", "external".
        /// </summary>
        IReadOnlySet<string> Tags { get; }

        /// <summary>
        /// Success:
        ///  - Probe is healthy.
        ///  - Optional diagnostics dictionary may be returned (null => nothing to report).
        ///
        /// Error:
        ///  - Probe is unhealthy.
        ///  - ResultError describes public message (ErrorMessage) and internal details (LogMessage/Exception).
        /// </summary>
        Task<Result<IReadOnlyDictionary<string, object>?>> CheckAsync(CancellationToken cancellationToken = default);
    }
}
