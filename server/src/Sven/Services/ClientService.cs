using Bureau;
using Bureau.Primitives.Errors;
using Bureau.Primitives.Features;
using Sven.Configurations;
using Sven.Data.Repositories;
using Sven;

namespace Sven.Services
{
    internal sealed class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        private readonly TimeProvider _timeProvider;
        private readonly IExternalProviderRegistry _externalProviderRegistry;

        public ClientService(IClientRepository clientRepository, TimeProvider timeProvider, IExternalProviderRegistry externalProviderRegistry)
        {
            _clientRepository = clientRepository;
            _timeProvider = timeProvider;
            _externalProviderRegistry = externalProviderRegistry;
        }

        public async Task<Result<Client>> CreateClientAsync(ClientRequest clientRequest, CancellationToken cancellationToken)
        {
            bool isConfidential =
                AuthConstants.OAuth.TokenAuthMethods.ClientSecretBasic.Equals(
                    clientRequest.TokenEndpointAuthMethod)
                || AuthConstants.OAuth.TokenAuthMethods.ClientSecretPost.Equals(
                    clientRequest.TokenEndpointAuthMethod);

            if (!isConfidential && !AuthConstants.OAuth.TokenAuthMethods.None.Equals(clientRequest.TokenEndpointAuthMethod))
            {
                return ResultError.From("Only supported client types are public and confidential", $"Received {nameof(clientRequest.TokenEndpointAuthMethod)} = {clientRequest.TokenEndpointAuthMethod};");
            }
            if (!DiscoveryService.ScopeSupported.IsScopeSameOrSubset(clientRequest.Scope))
            {
                return ResultError.From("Scope not supported", $"Received {nameof(clientRequest.Scope)} = {clientRequest.Scope};");
            }
            if (!isConfidential && (clientRequest.RedirectUris == null
                || clientRequest.RedirectUris.Count == 0
                || clientRequest.RedirectUris.Any(x => !UriValidator.IsRedirectUriValid(x))))
            {
                return ResultError.From("Redirect Uris are invalid", $"Received {nameof(clientRequest.RedirectUris)} = {string.Join(',', clientRequest.RedirectUris ?? [])};");
            }
            if (clientRequest.BureauFeatures != null)
            {
                HashSet<string> validFeatureKeys = new HashSet<string>(FeatureKeys.AllKeys);
                HashSet<string> validScopeKeys = new HashSet<string>(
                    _externalProviderRegistry.Providers
                        .SelectMany(p => _externalProviderRegistry.GetScopes(p.ProviderKey))
                        .Select(s => s.BureauKey));
                foreach (KeyValuePair<string, List<string>> feature in clientRequest.BureauFeatures)
                {
                    if (!validFeatureKeys.Contains(feature.Key))
                    {
                        return ResultError.From(
                            ProblemCodes.Request.InvalidPayload,
                            $"Feature key '{feature.Key}' is not a registered Bureau feature.");
                    }
                    foreach (string scopeKey in feature.Value)
                    {
                        if (!validScopeKeys.Contains(scopeKey))
                        {
                            return ResultError.From(
                                ProblemCodes.Request.InvalidPayload,
                                $"External scope key '{scopeKey}' is not registered in any provider.");
                        }
                    }
                }
            }
            string clientId = CreateNewClientId();
            Client client = new()
            {
                Identifier = clientId,
                AuthMethod = clientRequest.TokenEndpointAuthMethod,
                Type = isConfidential ? AuthConstants.ClientTypes.Confidential : AuthConstants.ClientTypes.Public,
                ClientUri = clientRequest.ClientUri,
                Contacts = clientRequest.Contacts,
                CreatedAt = _timeProvider.GetUtcNow(),
                GrantTypes = clientRequest.GrantTypes,
                Jwks = clientRequest.Jwks,
                JwksUri = clientRequest.JwksUri,
                LogoUri = clientRequest.LogoUri,
                Name = clientRequest.ClientName ?? clientId,
                PolicyUri = clientRequest.PolicyUri,
                RedirectUris = [.. clientRequest.RedirectUris ?? []],
                PostLogoutRedirectUris = clientRequest.PostLogoutRedirectUris,
                BureauFeatures = clientRequest.BureauFeatures,
                ResponseTypes = clientRequest.ResponseTypes,
                Scope = new ScopeParameter(clientRequest.Scope),
                SoftwareId = clientRequest.SoftwareId,
                SoftwareVersion = clientRequest.SoftwareVersion,
                TosUri = clientRequest.TosUri,
            };
            if (isConfidential)
            {
                byte[] secretBytes = new byte[32];
                System.Security.Cryptography.RandomNumberGenerator.Fill(secretBytes);
                string rawSecret = Convert.ToBase64String(secretBytes);
                client.HashedSecret = PasswordHasher.HashPassword(rawSecret);
                client.ClientSecret = rawSecret;
                client.ClientSecretExpiresAt = DateTimeOffset.FromUnixTimeSeconds(0);
            }
            Result result = await _clientRepository.StoreAsync(client, cancellationToken);
            if (result.IsError)
            {
                return result.Error;
            }
            return client;
        }

        public async Task<Result<Client>> GetClientAsync(string clientId, CancellationToken cancellationToken = default)
        {
            Result<Client> result = await _clientRepository.GetAsync(clientId, cancellationToken);
            if (result.IsError)
            {
                return result;
            }
            if (!result.Value.Active)
            {
                return ResultError.From("Client not active");
            }
            return result;
        }

        private static string CreateNewClientId()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}
