using Bureau;
using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Models;
using Sven;
using System.Security.Claims;
using System.Text.Json;

namespace Sven.Data.Stores
{
    internal class RefreshTokenService : IRefreshTokenService
    {
        private readonly SvenContext _context;
        private readonly TimeProvider _timeProvider;

        public RefreshTokenService(SvenContext context, TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }

        public async Task<Result<RefreshToken>> GetAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token))
                return ResultError.From("Token is empty.");

            RefreshTokenDb? db = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);

            if (db == null)
                return ResultError.From("Refresh token not found.");

            return MapToModel(db);
        }

        public async Task<Result> StoreAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            RefreshTokenDb? existing = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token.Token, cancellationToken);

            if (existing == null)
            {
                existing = new RefreshTokenDb { CreatedAt = _timeProvider.GetUtcNow() };
                _context.RefreshTokens.Add(existing);
            }

            existing.Token = token.Token;
            existing.ClientId = token.ClientId;
            existing.RedirectUri = token.RedirectUri;
            existing.Scope = token.Scope ?? string.Empty;
            existing.Nonce = token.Nonce;
            existing.ExpiresAt = token.ExpiresAt;
            existing.Claims = SerializeClaims(token.Claims);

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<Result> RemoveAsync(string token, CancellationToken cancellationToken = default)
        {
            RefreshTokenDb? db = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);

            if (db == null)
                return ResultError.From("Refresh token not found.");

            _context.RefreshTokens.Remove(db);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private static RefreshToken MapToModel(RefreshTokenDb db) => new RefreshToken
        {
            Token = db.Token,
            ClientId = db.ClientId,
            RedirectUri = db.RedirectUri,
            Scope = db.Scope,
            Nonce = db.Nonce,
            ExpiresAt = db.ExpiresAt,
            Claims = DeserializeClaims(db.Claims),
        };

        private static string SerializeClaims(List<Claim> claims)
        {
            var pairs = claims.Select(c => new ClaimPair(c.Type, c.Value)).ToList();
            return JsonSerializer.Serialize(pairs);
        }

        private static List<Claim> DeserializeClaims(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return [];
            var pairs = JsonSerializer.Deserialize<List<ClaimPair>>(json);
            return pairs?.Select(p => new Claim(p.Type, p.Value)).ToList() ?? [];
        }

        private record ClaimPair(string Type, string Value);
    }
}
