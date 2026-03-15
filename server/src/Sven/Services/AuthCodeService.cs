using Bureau;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sven.Configurations;
using Sven.Extensions;
using Sven;
using Sven.Models;
using System.Security.Claims;

namespace Sven.Services
{
    internal sealed class AuthCodeService : IAuthCodeService
    {
        private readonly InMemoryStore<string, AuthCode> _authCodeStore;
        private readonly IStore<string, OAuthRequest> _pkceRequestStore;
        private readonly TimeProvider _timeProvider;
        private readonly AuthOptions _authOptions;
        private readonly ILogger<AuthCodeService> _logger;

        public AuthCodeService(IOptions<AuthOptions> authOptions, TimeProvider timeProvider, IStore<string, OAuthRequest> pkceRequestStore, InMemoryStore<string, AuthCode> authCodeStore, ILogger<AuthCodeService> logger)
        {
            _authOptions = authOptions.Value;
            _timeProvider = timeProvider;
            _pkceRequestStore = pkceRequestStore;
            _authCodeStore = authCodeStore;
            _logger = logger;
        }

        public async Task<Result<string>> CreateOAuthRequestAsync(OAuthRequest request, CancellationToken cancellationToken)
        {
            string pkceKey = Guid.NewGuid().ToString("N");
            Result storeResult = await _pkceRequestStore.StoreAsync(pkceKey, request, cancellationToken);

            if (storeResult.IsError)
            {
                return storeResult.Error;
            }
            return new Result<string>(pkceKey);
        }

        public bool ExistsPkceKey(string pkceKey)
        {
            return _pkceRequestStore.Exists(pkceKey);
        }

        public Task<Result<OAuthRequest>> GetOAuthRequestAsync(string pkceKey, CancellationToken cancellationToken)
        {
            return _pkceRequestStore.GetAsync(pkceKey, cancellationToken);
        }

        public Task<Result> ClearOAuthRequestAsync(string pkceKey, CancellationToken cancellationToken)
        {
            return _pkceRequestStore.RemoveAsync(pkceKey, cancellationToken);
        }

        public async Task<Result<string>> CreateAuthCodeAsync(OAuthRequest request, List<Claim> claims, CancellationToken cancellationToken)
        {
            string code = Guid.NewGuid().ToString("N");

            AuthCode authCode = new AuthCode
            {
                Code = code,
                ClientId = request.ClientId,
                RedirectUri = request.RedirectUri,
                CodeChallenge = request.CodeChallenge,
                CodeChallengeMethod = request.CodeChallengeMethod,
                Scope = request.Scope,
                Claims = claims,
                ExpiresAt = _timeProvider.GetFutureTime(_authOptions.AuthorizationCodeLifetime),
                Nonce = request.Nonce
            };

            Result storeResult = await _authCodeStore.StoreAsync(code, authCode, cancellationToken);
            if (storeResult.IsError)
            {
                return storeResult.Error;
            }
            return new Result<string>(code);
        }

        public Task<Result<AuthCode>> GetAuthCodeAsync(string code, CancellationToken cancellationToken)
        {
            return _authCodeStore.GetAsync(code, cancellationToken);
        }

        public Task<Result> ClearAuthCodeAsync(string code, CancellationToken cancellationToken)
        {
            return _authCodeStore.RemoveAsync(code, cancellationToken);
        }

        public Task<Result<AuthCode>> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            if (_authCodeStore.TryRemoveAtomic(code, out AuthCode? authCode) && authCode != null)
            {
                return Task.FromResult(new Result<AuthCode>(authCode));
            }
            _logger.LogWarning("Security: auth code reuse or invalid code attempted for code={Code}", code);
            return Task.FromResult<Result<AuthCode>>(ResultError.From(AuthConstants.OAuth.Errors.InvalidGrant, "Authorization code not found or already used."));
        }

        public string GetCodeChallengeMethod(string? codeChallengeMethod)
        {
            return AuthConstants.OAuth.CodeChallengeMethods.Sha256.Equals(codeChallengeMethod, StringComparison.OrdinalIgnoreCase) ?
                AuthConstants.OAuth.CodeChallengeMethods.Sha256 : AuthConstants.OAuth.CodeChallengeMethods.Plain;
        }

        public bool IsAuthCodeExpired(AuthCode value)
        {
            return value.ExpiresAt > _timeProvider.GetUtcNow();
        }
    }
}
