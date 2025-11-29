using Niles.Chores.Abstractions.Services;
using Niles.Chores.Contexts;

namespace Niles.Chores.Services
{
    internal class ChoresHealthService : IChoresHealthService
    {
        private readonly ChoresContext _context;

        public ChoresHealthService(ChoresContext context)
        {
            _context = context;
        }

        public async Task<Boolean> IsHealthyAsync(CancellationToken cancellationToken)
        {
            // keep it super cheap
            return await _context.Database.CanConnectAsync(cancellationToken);
        }
    }
}
