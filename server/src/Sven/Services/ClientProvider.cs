using Bureau.Core;
using Sven.Data;
using Sven.Models;

namespace Sven.Services
{
    public class ClientProvider : IClientProvider
    {
        private readonly IClientStore _clientStore;

        public ClientProvider(IClientStore clientStore)
        {
            _clientStore = clientStore;
        }

        public async Task<Result<Client>> CreateClientAsync(ClientRequest clientRequest, CancellationToken cancellationToken)
        {
            Client client = new Client();
            // TODO set client
            Result result = await _clientStore.StoreAsync(client, cancellationToken);
            if (result.IsError)
            {
                return result.Error;
            }
            return client;
        }

        public async Task<Result<Client>> GetClientAsync(string clientId, CancellationToken cancellationToken = default)
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
