using Bureau;
using Sven;
using System.Security.Claims;

namespace Sven.Services
{
    public interface IAuthCodeService
    {
        Task<Result<string>> CreateOAuthRequestAsync(OAuthRequest request, CancellationToken cancellationToken);
        bool ExistsPkceKey(string pkceKey);
        Task<Result<OAuthRequest>> GetOAuthRequestAsync(string pkceKey, CancellationToken cancellationToken);
        Task<Result> ClearOAuthRequestAsync(string pkceKey, CancellationToken cancellationToken);
        Task<Result<string>> CreateAuthCodeAsync(OAuthRequest request, List<Claim> claims, CancellationToken cancellationToken);
        Task<Result<AuthCode>> GetAuthCodeAsync(string code, CancellationToken cancellationToken);
        Task<Result> ClearAuthCodeAsync(string code, CancellationToken cancellationToken);
        Task<Result<AuthCode>> ExchangeCodeAsync(string code, CancellationToken cancellationToken = default);
        string GetCodeChallengeMethod(string? codeChallengeMethod);
        bool IsAuthCodeExpired(AuthCode value);
    }
}
