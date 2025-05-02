using Bureau.Core;
using Sven.Models;

namespace Sven.Data
{
    public interface IClientStore
    {
        Task<Result<Client>> GetAsync(string clientId, CancellationToken cancellationToken = default);
        Task<Result> StoreAsync(Client client, CancellationToken cancellationToken = default);
    }
}
