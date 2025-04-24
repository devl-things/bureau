using Bureau.Core;
using Bureau.Core.Extensions;
using Microsoft.Extensions.Options;
using Sven.Configurations;
using Sven.Models;
using System.Security.Claims;

namespace Sven.Services
{
    public class AuthCodeProvider
    {
        private readonly IStore<string, AuthCode> _authCodeStore;
        private readonly IStore<string, OAuthRequest> _pkceRequestStore;
        private readonly TimeProvider _timeProvider;
        private readonly AuthOptions _authOptions;
        public AuthCodeProvider(IOptions<AuthOptions> authOptions, TimeProvider timeProvider, IStore<string, OAuthRequest> pkceRequestStore, IStore<string, AuthCode> authCodeStore)
        {
            _authOptions = authOptions.Value;
            _timeProvider = timeProvider;
            _pkceRequestStore = pkceRequestStore;
            _authCodeStore = authCodeStore;
        }

        internal async Task<Result<string>> CreateOAuthRequestAsync(OAuthRequest request, CancellationToken cancellationToken)
        {
            string pkceKey = Guid.NewGuid().ToString("N");
            Result storeResult = await _pkceRequestStore.StoreAsync(pkceKey, request, cancellationToken);

            if (storeResult.IsError)
            {
                return storeResult.Error;
            }
            return new Result<string>(pkceKey);
        }
        internal bool ExistsPkceKey(string pkceKey)
        {
            return _pkceRequestStore.Exists(pkceKey);
        }
        internal Task<Result<OAuthRequest>> GetOAuthRequestAsync(string pkceKey, CancellationToken cancellationToken)
        {
            return _pkceRequestStore.GetAsync(pkceKey, cancellationToken);
        }
        internal Task<Result> ClearOAuthRequestAsync(string pkceKey, CancellationToken cancellationToken)
        {
            return _pkceRequestStore.RemoveAsync(pkceKey, cancellationToken);
        }

        internal async Task<Result<string>> CreateAuthCodeAsync(OAuthRequest request, List<Claim> claims, CancellationToken cancellationToken)
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

        internal Task<Result<AuthCode>> GetAuthCodeAsync(string code, CancellationToken cancellationToken)
        {
            return _authCodeStore.GetAsync(code, cancellationToken);
        }
        internal Task<Result> ClearAuthCodeAsync(string code, CancellationToken cancellationToken)
        {
            return _authCodeStore.RemoveAsync(code, cancellationToken);
        }

        internal string GetCodeChallengeMethod(string? codeChallengeMethod)
        {
            return AuthConstants.OAuth.CodeChallengeMethods.Sha256.Equals(codeChallengeMethod, StringComparison.OrdinalIgnoreCase) ?
                AuthConstants.OAuth.CodeChallengeMethods.Sha256 : AuthConstants.OAuth.CodeChallengeMethods.Plain;
        }

        internal bool IsAuthCodeExpired(AuthCode value)
        {
            return value.ExpiresAt > _timeProvider.GetUtcNow();
        }
    }
}
