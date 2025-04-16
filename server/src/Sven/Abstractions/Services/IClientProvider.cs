using Bureau.Core;

namespace Sven.Abstractions.Services
{
    public interface IClientProvider
    {
        Task<Result<bool>> IsValidAsync(string clientId, string redirectUri, string scope, CancellationToken cancellationToken = default);
    }
}
