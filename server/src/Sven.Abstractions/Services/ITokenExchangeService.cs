using Bureau;
using Sven.Models;

namespace Sven.Services
{
    public interface ITokenExchangeService
    {
        Task<Result<TokenExchangeResponse>> ExchangeAsync(
            string clientId,
            string subjectToken,
            string provider,
            string featureKey,
            CancellationToken cancellationToken = default);
    }
}
