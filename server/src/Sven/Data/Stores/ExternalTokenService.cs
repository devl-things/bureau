using Bureau;
using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Models;
using Sven;
using Sven.Services;

namespace Sven.Data.Stores
{
    internal class ExternalTokenService : IExternalTokenService
    {
        private readonly SvenContext _context;
        private readonly ISymEncryptor _encryptor;
        private readonly TimeProvider _timeProvider;

        public ExternalTokenService(SvenContext context, ISymEncryptor encryptor, TimeProvider timeProvider)
        {
            _context = context;
            _encryptor = encryptor;
            _timeProvider = timeProvider;
        }

        public async Task<Result<UserExternalToken>> GetAsync(string userId, string provider, string externalAccountId, CancellationToken cancellationToken = default)
        {
            UserExternalTokenDb? db = await _context.UserExternalTokens
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == provider && x.ExternalAccountId == externalAccountId, cancellationToken);

            if (db == null)
                return ResultError.From($"No external token found for user {userId} provider {provider} account {externalAccountId}.");

            return MapToModel(db);
        }

        public async Task<Result> StoreAsync(UserExternalToken token, CancellationToken cancellationToken = default)
        {
            UserExternalTokenDb? existing = await _context.UserExternalTokens
                .FirstOrDefaultAsync(x => x.UserId == token.UserId && x.Provider == token.Provider && x.ExternalAccountId == token.ExternalAccountId, cancellationToken);

            if (existing == null)
            {
                existing = new UserExternalTokenDb { LinkedAt = _timeProvider.GetUtcNow() };
                _context.UserExternalTokens.Add(existing);
            }

            Result<string> encryptedAccess = _encryptor.Encrypt(token.AccessToken);
            if (encryptedAccess.IsError) return encryptedAccess.Error;

            existing.UserId = token.UserId;
            existing.Provider = token.Provider;
            existing.ExternalAccountId = token.ExternalAccountId;
            existing.Scopes = token.Scopes;
            existing.AccessToken = encryptedAccess.Value;
            existing.ExpiresAt = token.ExpiresAt;
            existing.LastRefreshedAt = token.LastRefreshedAt;
            existing.RequiresReauthorisation = token.RequiresReauthorisation;

            if (token.RefreshToken != null)
            {
                Result<string> encryptedRefresh = _encryptor.Encrypt(token.RefreshToken);
                if (encryptedRefresh.IsError) return encryptedRefresh.Error;
                existing.RefreshToken = encryptedRefresh.Value;
            }
            else
            {
                existing.RefreshToken = null;
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IReadOnlyList<UserExternalToken>> GetExpiringSoonAsync(DateTimeOffset threshold, CancellationToken cancellationToken = default)
        {
            List<UserExternalTokenDb> rows = await _context.UserExternalTokens
                .Where(x => !x.RequiresReauthorisation && x.RefreshToken != null && x.ExpiresAt <= threshold)
                .ToListAsync(cancellationToken);

            return rows.Select(MapToModel).ToList();
        }

        public async Task<Result> MarkReauthRequiredAsync(string userId, string provider, string externalAccountId, CancellationToken cancellationToken = default)
        {
            UserExternalTokenDb? db = await _context.UserExternalTokens
                .FirstOrDefaultAsync(x => x.UserId == userId && x.Provider == provider && x.ExternalAccountId == externalAccountId, cancellationToken);

            if (db == null)
                return ResultError.From($"No external token found for user {userId} provider {provider} account {externalAccountId}.");

            db.RequiresReauthorisation = true;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private UserExternalToken MapToModel(UserExternalTokenDb db)
        {
            Result<string> access = _encryptor.DecryptString(db.AccessToken);
            string? refresh = db.RefreshToken != null
                ? _encryptor.DecryptString(db.RefreshToken).Value
                : null;

            return new UserExternalToken
            {
                UserId = db.UserId,
                Provider = db.Provider,
                ExternalAccountId = db.ExternalAccountId,
                Scopes = db.Scopes,
                AccessToken = access.Value,
                RefreshToken = refresh,
                ExpiresAt = db.ExpiresAt,
                LinkedAt = db.LinkedAt,
                LastRefreshedAt = db.LastRefreshedAt,
                RequiresReauthorisation = db.RequiresReauthorisation,
            };
        }
    }
}
