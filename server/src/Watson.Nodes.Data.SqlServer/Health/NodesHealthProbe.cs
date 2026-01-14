using Bureau;
using Bureau.Primitives.Health;
using System.Diagnostics;
using Watson.Nodes.Contexts;

namespace Watson.Nodes.Data.SqlServer.Health
{
    internal class NodesHealthProbe : IHealthProbe
    {
        private readonly NodesContext _context;

        public NodesHealthProbe(NodesContext context)
        {
            _context = context;
        }

        public string Key => "nodes-db";

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
