using Bureau.Core;

namespace Sven.Abstractions.Services
{
    public interface IClientProvider
    {
        Task<Result<bool>> IsValidAsync(string? clientId, CancellationToken cancellationToken = default);
        Task<Result<bool>> IsValidAsync(string clientId, string redirectUri, CancellationToken cancellationToken = default);
        Task<Result<bool>> IsScopeValidAsync(string clientId, string scope, CancellationToken cancellationToken = default);
    }
}
