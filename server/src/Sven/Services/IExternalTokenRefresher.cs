using Sven;

namespace Sven.Services
{
    internal interface IExternalTokenRefresher
    {
        Task RefreshAsync(UserExternalToken token, CancellationToken cancellationToken = default);
    }
}
