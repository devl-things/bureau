using Bureau;
using Sven.Models;

namespace Sven.Data.Repositories
{
    internal interface IClientRepository
    {
        Task<Result<Client>> GetAsync(string clientId, CancellationToken cancellationToken = default);
        Task<Result> StoreAsync(Client client, CancellationToken cancellationToken = default);
    }
}
