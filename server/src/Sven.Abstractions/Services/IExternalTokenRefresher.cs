using Sven;

namespace Sven.Services
{
    public interface IExternalTokenRefresher
    {
        Task RefreshAsync(UserExternalToken token, CancellationToken cancellationToken = default);
    }
}
