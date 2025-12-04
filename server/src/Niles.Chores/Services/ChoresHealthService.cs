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

        public async Task<bool> IsHealthyAsync(CancellationToken cancellationToken)
        {
            // keep it super cheap
            return await _context.Database.CanConnectAsync(cancellationToken);
        }
    }
}
