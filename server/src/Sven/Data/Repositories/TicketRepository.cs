using Bureau;
using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Models;

namespace Sven.Data.Repositories
{
    internal sealed class TicketRepository
    {
        private readonly SvenContext _context;
        private readonly TimeProvider _timeProvider;

        public TicketRepository(SvenContext context, TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }

        public async Task<Result> StoreAsync(string ticket, string userId, CancellationToken cancellationToken = default)
        {
            TicketDb db = new TicketDb
            {
                Ticket = ticket,
                UserId = userId,
                ExpiresAt = _timeProvider.GetUtcNow().AddMinutes(5),
                CreatedAt = _timeProvider.GetUtcNow()
            };
            _context.Tickets.Add(db);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<Result<string>> GetAsync(string ticket, CancellationToken cancellationToken = default)
        {
            TicketDb? db = await _context.Tickets
                .FirstOrDefaultAsync(x => x.Ticket == ticket, cancellationToken);

            if (db == null)
            {
                return ResultError.From("Ticket not found.");
            }

            if (db.ExpiresAt <= _timeProvider.GetUtcNow())
            {
                return ResultError.From("Ticket has expired.");
            }

            return new Result<string>(db.UserId);
        }

        public async Task<Result> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _context.Tickets
                .Where(x => x.Ticket == key)
                .ExecuteDeleteAsync(cancellationToken);
            return true;
        }
    }
}
