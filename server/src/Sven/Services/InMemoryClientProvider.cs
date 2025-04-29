using Bureau.Core;
using Sven.Abstractions.Services;
using Sven.Models;

namespace Sven.Services
{
    public class InMemoryClientProvider : IClientProvider
    {
        private readonly IStore<string, Client> _clientStore;

        public InMemoryClientProvider(IStore<string, Client> clientStore)
        {
            _clientStore = clientStore;
        }
        public async Task<Result<Client>> GetClientAsync(string clientId, CancellationToken cancellationToken)
        {
            Result<Client> result = await _clientStore.GetAsync(clientId, cancellationToken);
            if (result.IsError)
            {
                return result;
            }
            if (!result.Value.Active)
            {
                return new ResultError("Client not active");
            }
            return result;
        }
    }
}
