using Bureau.Core;
using Bureau.Core.Extensions;
using Microsoft.Extensions.Options;
using Sven.Configurations;
using Sven.Models;
using System.Security.Claims;
using System.Text;

namespace Sven.Services
{
    public class AuthCodeProvider
    {
        private readonly ILogger<AuthCodeProvider> _logger;
        private readonly IStore<string, AuthCode> _authCodeStore;
        private readonly TimeProvider _timeProvider;
        private readonly AuthOptions _authOptions;
        public AuthCodeProvider(ILogger<AuthCodeProvider> logger, IOptions<AuthOptions> authOptions, IStore<string, AuthCode> authCodeStore, TimeProvider timeProvider)
        {
            _logger = logger;
            _authCodeStore = authCodeStore;
            _timeProvider = timeProvider;
            _authOptions = authOptions.Value;
        }

        internal Task<Result> ClearAsync(string code, CancellationToken cancellationToken)
        {
            return _authCodeStore.RemoveAsync(code, cancellationToken);
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
                ExpiresAt = _timeProvider.GetFutureTime(new TimeSpan(0, 5, 0)),
                Nonce = request.Nonce
            };

            Result storeResult = await _authCodeStore.StoreAsync(code, authCode, cancellationToken);
            if (storeResult.IsError)
            {
                _logger.LogResultError(storeResult.Error);
                return new ResultError(AuthConstants.OAuth.Errors.ServerError, "Failed create code.");
            }
            return new Result<string>(code);
        }

        internal Task<Result<AuthCode>> GetAsync(string code, CancellationToken cancellationToken)
        {
            return _authCodeStore.GetAsync(code, cancellationToken);
        }

        internal Result<bool> IsAuthCodeValid(AuthCode authCode, string clientId, string redirectUri, string codeVerifier)
        {
            string hashed = GenerateCodeChallenge(codeVerifier);
            return !(authCode.ClientId != clientId || authCode.RedirectUri != redirectUri || authCode.ExpiresAt < _timeProvider.GetUtcNow() || authCode.CodeChallenge != hashed);
        }

        internal static string GenerateCodeChallenge(string verifier)
        {
            return Base64CodeEncode(System.Security.Cryptography.SHA256.HashData(Encoding.ASCII.GetBytes(verifier)));
        }

        internal static string Base64CodeEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        internal bool IsCodeChallengeValid(string code_challenge, string code_challenge_method)
        {
            return AuthConstants.OAuth.CodeChallengeMethods.Sha256.Equals(code_challenge_method, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(code_challenge);
        }
    }
}
