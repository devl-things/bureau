using Bureau.Core;
using Sven.Models;

namespace Sven.Abstractions.Services
{
    public interface IClientStore
    {
        Task<Result<Client>> GetAsync(string clientId, CancellationToken cancellationToken);
    }
}
