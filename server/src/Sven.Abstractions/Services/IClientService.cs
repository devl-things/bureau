using Bureau;
using Sven.Models;

namespace Sven.Services
{
    public interface IClientService
    {
        Task<Result<Client>> CreateClientAsync(ClientRequest clientRequest, CancellationToken cancellationToken);
        Task<Result<Client>> GetClientAsync(string clientId, CancellationToken cancellationToken = default);
    }
}
