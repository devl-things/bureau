using Bureau;
using Bureau.Primitives.Health;
using Sven.Data.Contexts;
using System.Diagnostics;

namespace Sven.Data.Postgres.Health
{
    internal sealed class SvenHealthProbe : IHealthProbe
    {
        private readonly SvenContext _context;

        public SvenHealthProbe(SvenContext context)
        {
            _context = context;
        }

        public string Key
        {
            get { return "sven-db"; }
        }

        public IReadOnlySet<string> Tags { get; } =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ProbeTags.Database,
                ProbeTags.Critical
            };

        public async Task<Result<IReadOnlyDictionary<string, object>?>> CheckAsync(CancellationToken cancellationToken = default)
        {
            Stopwatch sw = Stopwatch.StartNew();

            bool ok = await _context.Database.CanConnectAsync(cancellationToken);

            sw.Stop();

            if (ok)
            {
                return new Result<IReadOnlyDictionary<string, object>?>(null);
            }

            return ResultError.From(code: "db_unreachable", errorMessage: "Database is not reachable.",
                logMessage: $"Probe '{Key}' failed. ElapsedMs={sw.ElapsedMilliseconds}. Provider={_context.Database.ProviderName}");
        }
    }
}
