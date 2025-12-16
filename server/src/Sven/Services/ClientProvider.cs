using Bureau;
using Sven.Configurations;
using Sven.Data;
using Sven.Models;

namespace Sven.Services
{
    public class ClientProvider : IClientProvider
    {
        private readonly IClientStore _clientStore;
        private readonly TimeProvider _timeProvider;

        public ClientProvider(IClientStore clientStore, TimeProvider timeProvider)
        {
            _clientStore = clientStore;
            _timeProvider = timeProvider;
        }

        public async Task<Result<Client>> CreateClientAsync(ClientRequest clientRequest, CancellationToken cancellationToken)
        {
            if (!AuthConstants.OAuth.TokenAuthMethods.None.Equals(clientRequest.TokenEndpointAuthMethod))
            {
                return new ResultError("Only supported client type is public", $"Received {nameof(clientRequest.TokenEndpointAuthMethod)} = {clientRequest.TokenEndpointAuthMethod};");
            }
            if (DiscoveryService.ScopeSupported.IsScopeSameOrSubset(clientRequest.Scope))
            {
                return new ResultError("Scope not supported", $"Received {nameof(clientRequest.Scope)} = {clientRequest.Scope};");
            }
            if (clientRequest.RedirectUris.Count == 0 || clientRequest.RedirectUris.Any(x => !UriValidator.IsRedirectUriValid(x)))
            {
                return new ResultError("Redirect Uris are invalid", $"Received {nameof(clientRequest.RedirectUris)} = {string.Join(',', clientRequest.RedirectUris)};");
            }
            string clientId = CreateNewClientId();
            Client client = new()
            {
                Identifier = clientId,
                AuthMethod = clientRequest.TokenEndpointAuthMethod,
                // #38 ClientSecret, ClientSecretExpiresAt
                ClientUri = clientRequest.ClientUri,
                Contacts = clientRequest.Contacts,
                CreatedAt = _timeProvider.GetUtcNow(),
                GrantTypes = clientRequest.GrantTypes,
                Jwks = clientRequest.Jwks,
                JwksUri = clientRequest.JwksUri,
                LogoUri = clientRequest.LogoUri,
                Name = clientRequest.ClientName ?? clientId,
                PolicyUri = clientRequest.PolicyUri,
                RedirectUris = [.. clientRequest.RedirectUris],
                ResponseTypes = clientRequest.ResponseTypes,
                Scope = new ScopeParameter(clientRequest.Scope),
                SoftwareId = clientRequest.SoftwareId,
                SoftwareVersion = clientRequest.SoftwareVersion,
                TosUri = clientRequest.TosUri,
            };
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

        private static string CreateNewClientId()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
