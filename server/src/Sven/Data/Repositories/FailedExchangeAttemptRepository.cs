using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Sven.Configurations;
using Sven.Data.Contexts;
using Sven.Data.Models;

namespace Sven.Data.Repositories
{
    internal sealed class FailedExchangeAttemptRepository
    {
        private readonly SvenContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly TokenExchangeOptions _options;

        public FailedExchangeAttemptRepository(SvenContext context, TimeProvider timeProvider, IOptions<TokenExchangeOptions> options)
        {
            _context = context;
            _timeProvider = timeProvider;
            _options = options.Value;
        }

        public async Task<bool> IsLockedOutAsync(string clientId, string userId, CancellationToken cancellationToken = default)
        {
            FailedExchangeAttemptDb? db = await _context.FailedExchangeAttempts
                .FirstOrDefaultAsync(x => x.ClientId == clientId && x.UserId == userId, cancellationToken);

            if (db == null)
            {
                return false;
            }

            if (db.FailureCount < _options.MaxFailuresBeforeLockout)
            {
                return false;
            }

            if (db.LockoutStartedAt == null)
            {
                return false;
            }

            DateTimeOffset lockoutExpiry = db.LockoutStartedAt.Value + _options.LockoutDuration;
            if (_timeProvider.GetUtcNow() >= lockoutExpiry)
            {
                await _context.FailedExchangeAttempts
                    .Where(x => x.ClientId == clientId && x.UserId == userId)
                    .ExecuteDeleteAsync(cancellationToken);
                return false;
            }

            return true;
        }

        public async Task RecordFailureAsync(string clientId, string userId, CancellationToken cancellationToken = default)
        {
            FailedExchangeAttemptDb? db = await _context.FailedExchangeAttempts
                .FirstOrDefaultAsync(x => x.ClientId == clientId && x.UserId == userId, cancellationToken);

            if (db == null)
            {
                FailedExchangeAttemptDb newDb = new FailedExchangeAttemptDb
                {
                    ClientId = clientId,
                    UserId = userId,
                    FailureCount = 1,
                    LockoutStartedAt = null
                };
                _context.FailedExchangeAttempts.Add(newDb);
            }
            else
            {
                db.FailureCount += 1;
                if (db.FailureCount >= _options.MaxFailuresBeforeLockout && db.LockoutStartedAt == null)
                {
                    db.LockoutStartedAt = _timeProvider.GetUtcNow();
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task ClearAsync(string clientId, string userId, CancellationToken cancellationToken = default)
        {
            await _context.FailedExchangeAttempts
                .Where(x => x.ClientId == clientId && x.UserId == userId)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
