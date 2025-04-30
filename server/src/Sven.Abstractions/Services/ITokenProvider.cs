using Bureau.Core;
using Sven.Models;

namespace Sven.Services
{
    public interface ITokenProvider
    {
        Task<Result<SvenToken>> CreateTokenAsync(ClientClaims clientClaims, CancellationToken cancellationToken);
        Task<Result<SvenToken>> CreateTokenAsync(string refreshToken, string? scope, CancellationToken cancellationToken);
        Task<Result<bool>> IsRefreshTokenValidAsync(string refreshToken, string clientId, string redirectUri, string? scope, CancellationToken cancellationToken);
        Task<Result<bool>> RevokeAsync(string token, string clientId, string? tokenTypeHint, CancellationToken cancellationToken);
    }
}
