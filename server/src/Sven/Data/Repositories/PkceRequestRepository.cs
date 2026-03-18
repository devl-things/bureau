using Bureau;
using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Models;
using Sven.Models;

namespace Sven.Data.Repositories
{
    internal sealed class PkceRequestRepository
    {
        private readonly SvenContext _context;
        private readonly TimeProvider _timeProvider;

        public PkceRequestRepository(SvenContext context, TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }

        public async Task<Result> StoreAsync(string pkceKey, OAuthRequest request, CancellationToken cancellationToken = default)
        {
            OAuthRequestDb db = new OAuthRequestDb
            {
                PkceKey = pkceKey,
                ClientId = request.ClientId,
                RedirectUri = request.RedirectUri,
                Scope = request.Scope,
                CodeChallenge = request.CodeChallenge,
                CodeChallengeMethod = request.CodeChallengeMethod,
                State = request.State,
                Nonce = request.Nonce,
                ExpiresAt = _timeProvider.GetUtcNow().AddMinutes(10),
                CreatedAt = _timeProvider.GetUtcNow()
            };
            _context.OAuthRequests.Add(db);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<Result<OAuthRequest>> GetAsync(string pkceKey, CancellationToken cancellationToken = default)
        {
            OAuthRequestDb? db = await _context.OAuthRequests
                .FirstOrDefaultAsync(x => x.PkceKey == pkceKey, cancellationToken);

            if (db == null)
            {
                return ResultError.From("OAuth request not found.");
            }

            return MapToDomain(db);
        }

        public async Task<Result> RemoveAsync(string pkceKey, CancellationToken cancellationToken = default)
        {
            await _context.OAuthRequests
                .Where(x => x.PkceKey == pkceKey)
                .ExecuteDeleteAsync(cancellationToken);
            return true;
        }

        public Task<bool> ExistsAsync(string pkceKey, CancellationToken cancellationToken = default)
        {
            return _context.OAuthRequests.AnyAsync(x => x.PkceKey == pkceKey, cancellationToken);
        }

        private static OAuthRequest MapToDomain(OAuthRequestDb db)
        {
            OAuthRequest model = new OAuthRequest
            {
                ClientId = db.ClientId,
                RedirectUri = db.RedirectUri ?? string.Empty,
                Scope = db.Scope ?? string.Empty,
                CodeChallenge = db.CodeChallenge ?? string.Empty,
                CodeChallengeMethod = db.CodeChallengeMethod ?? string.Empty,
                State = db.State,
                Nonce = db.Nonce
            };
            return model;
        }
    }
}
