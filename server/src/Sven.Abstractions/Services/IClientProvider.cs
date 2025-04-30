using Bureau.Core;
using Sven.Models;

namespace Sven.Services
{
    public interface IClientProvider
    {
        Task<Result<Client>> GetClientAsync(string clientId, CancellationToken cancellationToken);
    }
}
