using Bureau.Core;
using Sven.Configurations;
using Sven.Models;
using System.Security.Claims;
using System.Text;

namespace Sven.Services
{
    public class AuthCodeProvider
    {
        private readonly IStore<string, AuthCode> _authCodeStore;
        private readonly TimeProvider _timeProvider;
        public AuthCodeProvider(IStore<string, AuthCode> authCodeStore, TimeProvider timeProvider)
        {
            _authCodeStore = authCodeStore;
            _timeProvider = timeProvider;
        }

        internal Task<Result> ClearAsync(string code, CancellationToken cancellationToken)
        {
            return _authCodeStore.RemoveAsync(code, cancellationToken);
        }

        internal async Task<string> CreateAuthCodeAsync(OAuthRequest request, List<Claim> claims, CancellationToken cancellationToken)
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
                ExpiresAt = _timeProvider.GetUtcNow().AddMinutes(5)
            };

            await _authCodeStore.StoreAsync(code, authCode, cancellationToken);

            return code;
        }

        internal Task<Result<AuthCode>> GetAsync(string code, CancellationToken cancellationToken)
        {
            return _authCodeStore.GetAsync(code, cancellationToken);
        }

        internal Result<bool> IsAuthCodeValid(AuthCode authCode, string clientId, string codeVerifier)
        {
            string hashed = GenerateCodeChallenge(codeVerifier);
            return !(authCode.ClientId != clientId || authCode.ExpiresAt < _timeProvider.GetUtcNow() || authCode.CodeChallenge != hashed);
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
