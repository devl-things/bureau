using Bureau;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sven.Configurations;
using Sven.Data.Repositories;
using Sven.Extensions;
using Sven;
using Sven.Models;
using System.Security.Claims;

namespace Sven.Services
{
    internal sealed class AuthCodeService : IAuthCodeService
    {
        private readonly AuthCodeRepository _authCodeRepository;
        private readonly PkceRequestRepository _pkceRequestRepository;
        private readonly TimeProvider _timeProvider;
        private readonly AuthOptions _authOptions;
        private readonly ILogger<AuthCodeService> _logger;

        public AuthCodeService(IOptions<AuthOptions> authOptions, TimeProvider timeProvider, PkceRequestRepository pkceRequestRepository, AuthCodeRepository authCodeRepository, ILogger<AuthCodeService> logger)
        {
            _authOptions = authOptions.Value;
            _timeProvider = timeProvider;
            _pkceRequestRepository = pkceRequestRepository;
            _authCodeRepository = authCodeRepository;
            _logger = logger;
        }

        public async Task<Result<string>> CreateOAuthRequestAsync(OAuthRequest request, CancellationToken cancellationToken)
        {
            string pkceKey = Guid.NewGuid().ToString("N");
            Result storeResult = await _pkceRequestRepository.StoreAsync(pkceKey, request, cancellationToken);

            if (storeResult.IsError)
            {
                return storeResult.Error;
            }
            return new Result<string>(pkceKey);
        }

        public bool ExistsPkceKey(string pkceKey)
        {
            return _pkceRequestRepository.ExistsAsync(pkceKey, CancellationToken.None).GetAwaiter().GetResult();
        }

        public Task<Result<OAuthRequest>> GetOAuthRequestAsync(string pkceKey, CancellationToken cancellationToken)
        {
            return _pkceRequestRepository.GetAsync(pkceKey, cancellationToken);
        }

        public Task<Result> ClearOAuthRequestAsync(string pkceKey, CancellationToken cancellationToken)
        {
            return _pkceRequestRepository.RemoveAsync(pkceKey, cancellationToken);
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

            Result storeResult = await _authCodeRepository.StoreAsync(code, authCode, cancellationToken);
            if (storeResult.IsError)
            {
                return storeResult.Error;
            }
            return new Result<string>(code);
        }

        public Task<Result<AuthCode>> GetAuthCodeAsync(string code, CancellationToken cancellationToken)
        {
            return _authCodeRepository.GetAsync(code, cancellationToken);
        }

        public Task<Result> ClearAuthCodeAsync(string code, CancellationToken cancellationToken)
        {
            return _authCodeRepository.RemoveAsync(code, cancellationToken);
        }

        public Task<Result<AuthCode>> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default)
        {
            return _authCodeRepository.ExchangeCodeAsync(code, cancellationToken);
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
