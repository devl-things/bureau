using Bureau;
using Sven.Models;

namespace Sven.Data
{
    public interface IRefreshTokenService
    {
        Task<Result<RefreshToken>> GetAsync(string token, CancellationToken cancellationToken = default);
        Task<Result> StoreAsync(RefreshToken token, CancellationToken cancellationToken = default);
        Task<Result> RemoveAsync(string token, CancellationToken cancellationToken = default);
    }
}
