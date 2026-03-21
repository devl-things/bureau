using Bureau;
using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Models;
using Sven.Models;
using System.Security.Claims;
using System.Text.Json;

namespace Sven.Data.Repositories
{
    internal sealed class RefreshTokenRepository
    {
        private readonly SvenContext _context;
        private readonly TimeProvider _timeProvider;

        public RefreshTokenRepository(SvenContext context, TimeProvider timeProvider)
        {
            _context = context;
            _timeProvider = timeProvider;
        }

        public async Task<Result<RefreshToken>> GetAsync(string token, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return ResultError.From("Refresh token is empty.");
            }

            RefreshTokenDb? db = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);

            if (db == null)
            {
                return ResultError.From("Refresh token not found.");
            }

            return MapToDomain(db);
        }

        public async Task<Result> StoreAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshTokenDb db = new RefreshTokenDb
            {
                Token = refreshToken.Token,
                ClientId = refreshToken.ClientId,
                RedirectUri = refreshToken.RedirectUri,
                Scope = refreshToken.Scope ?? string.Empty,
                Nonce = refreshToken.Nonce,
                ExpiresAt = refreshToken.ExpiresAt,
                Claims = SerializeClaims(refreshToken.Claims),
                CreatedAt = _timeProvider.GetUtcNow()
            };
            _context.RefreshTokens.Add(db);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<Result> RemoveAsync(string token, CancellationToken cancellationToken = default)
        {
            await _context.RefreshTokens
                .Where(x => x.Token == token)
                .ExecuteDeleteAsync(cancellationToken);
            return true;
        }

        private static RefreshToken MapToDomain(RefreshTokenDb db)
        {
            RefreshToken model = new RefreshToken();
            model.Token = db.Token;
            model.ClientId = db.ClientId;
            model.RedirectUri = db.RedirectUri;
            model.Scope = db.Scope;
            model.Nonce = db.Nonce;
            model.ExpiresAt = db.ExpiresAt;
            model.Claims = DeserializeClaims(db.Claims);
            return model;
        }

        private static string SerializeClaims(List<Claim> claims)
        {
            List<ClaimPair> pairs = claims.Select(c => new ClaimPair(c.Type, c.Value)).ToList();
            return JsonSerializer.Serialize(pairs);
        }

        private static List<Claim> DeserializeClaims(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }
            List<ClaimPair>? pairs = JsonSerializer.Deserialize<List<ClaimPair>>(json);
            return pairs?.Select(p => new Claim(p.Type, p.Value)).ToList() ?? [];
        }

        private record ClaimPair(string Type, string Value);
    }
}
