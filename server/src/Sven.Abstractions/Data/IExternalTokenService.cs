using Bureau;
using Sven;

namespace Sven.Data
{
    public interface IExternalTokenService
    {
        Task<Result<UserExternalToken>> GetAsync(string userId, string provider, string externalAccountId, CancellationToken cancellationToken = default);
        Task<Result> StoreAsync(UserExternalToken token, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<UserExternalToken>> GetExpiringSoonAsync(DateTimeOffset threshold, CancellationToken cancellationToken = default);
        Task<Result> MarkReauthRequiredAsync(string userId, string provider, string externalAccountId, CancellationToken cancellationToken = default);
    }
}
