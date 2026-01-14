using Bureau.Primitives;
using Watson.Nodes.Contexts;

namespace Watson.Nodes.Data.SqlServer.Health
{
    public class NodesHealthProbe : IHealthProbe
    {
        private readonly NodesContext _context;

        public NodesHealthProbe(NodesContext context)
        {
            _context = context;
        }
        public Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
        {
            // keep it super cheap
            return await _context.Database.CanConnectAsync(cancellationToken);
        }
    }
}
