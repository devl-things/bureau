using Bureau.Core;
using Sven.Models;

namespace Sven.Abstractions.Services
{
    public interface IClientProvider
    {
        Task<Result<Client>> GetClientAsync(string clientId, CancellationToken cancellationToken);
    }
}
