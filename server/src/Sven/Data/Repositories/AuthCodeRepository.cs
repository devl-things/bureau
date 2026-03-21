using Bureau;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sven.Configurations;
using Sven.Data.Contexts;
using Sven.Data.Models;
using Sven.Models;
using System.Security.Claims;
using System.Text.Json;

namespace Sven.Data.Repositories
{
    internal sealed class AuthCodeRepository
    {
        private readonly SvenContext _context;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<AuthCodeRepository> _logger;

        public AuthCodeRepository(SvenContext context, TimeProvider timeProvider, ILogger<AuthCodeRepository> logger)
        {
            _context = context;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<Result> StoreAsync(string code, AuthCode authCode, CancellationToken cancellationToken = default)
        {
            AuthCodeDb db = new AuthCodeDb
            {
                Code = code,
                ClientId = authCode.ClientId,
                RedirectUri = authCode.RedirectUri,
                Scope = authCode.Scope ?? string.Empty,
                CodeChallenge = authCode.CodeChallenge,
                CodeChallengeMethod = authCode.CodeChallengeMethod,
                Nonce = authCode.Nonce,
                Claims = SerializeClaims(authCode.Claims),
                ExpiresAt = authCode.ExpiresAt,
                CreatedAt = _timeProvider.GetUtcNow()
            };
            _context.AuthCodes.Add(db);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<Result<AuthCode>> GetAsync(string code, CancellationToken cancellationToken = default)
        {
            AuthCodeDb? db = await _context.AuthCodes
                .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

            if (db == null)
            {
                return ResultError.From("Authorization code not found.");
            }

            if (db.ExpiresAt <= _timeProvider.GetUtcNow())
            {
                return ResultError.From("Authorization code has expired.");
            }

            return MapToDomain(db);
        }

        public async Task<Result> RemoveAsync(string code, CancellationToken cancellationToken = default)
        {
            await _context.AuthCodes
                .Where(x => x.Code == code)
                .ExecuteDeleteAsync(cancellationToken);
            return true;
        }

        public async Task<Result<AuthCode>> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            AuthCodeDb? db = await _context.AuthCodes
                .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

            if (db == null)
            {
                _logger.LogWarning("Security: auth code reuse or invalid code attempted for code={Code}", code);
                return ResultError.From(AuthConstants.OAuth.Errors.InvalidGrant, "Authorization code not found or already used.");
            }

            int rows = await _context.AuthCodes
                .Where(x => x.Code == code)
                .ExecuteDeleteAsync(cancellationToken);

            if (rows == 0)
            {
                _logger.LogWarning("Security: auth code race condition detected for code={Code}", code);
                return ResultError.From(AuthConstants.OAuth.Errors.InvalidGrant, "Authorization code not found or already used.");
            }

            return MapToDomain(db);
        }

        public Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default)
        {
            return _context.AuthCodes.AnyAsync(x => x.Code == code, cancellationToken);
        }

        private static AuthCode MapToDomain(AuthCodeDb db)
        {
            AuthCode model = new AuthCode
            {
                Code = db.Code,
                ClientId = db.ClientId,
                RedirectUri = db.RedirectUri,
                Scope = db.Scope,
                CodeChallenge = db.CodeChallenge ?? string.Empty,
                CodeChallengeMethod = db.CodeChallengeMethod ?? string.Empty,
                Nonce = db.Nonce,
                ExpiresAt = db.ExpiresAt,
                Claims = DeserializeClaims(db.Claims)
            };
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
