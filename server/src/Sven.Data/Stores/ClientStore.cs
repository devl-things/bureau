using Bureau;
using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Models;
using Sven.Models;
using System.Text.Json;

namespace Sven.Data.Stores
{
    internal class ClientStore : IClientStore
    {
        private readonly SvenContext _context;
        public ClientStore(SvenContext context)
        {
            _context = context;
        }
        public async Task<Result<Client>> GetAsync(string clientId, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return ResultError.From("Key cannot be empty");
            }
            ClientDb? client = await GetClientDbAsync(clientId, cancellationToken);
            if (client == null)
            {
                return ResultError.From("Client doesn't exist.");
            }
            IClientAddendum? addendum = JsonSerializer.Deserialize<ClientAddendum>(client.ClientAddendum.Data);
            return ToClient(client, addendum);
        }

        private static Client ToClient(ClientDb client, IClientAddendum? addendum)
        {

            return new Client()
            {
                Active = client.Active,
                AuthMethod = addendum?.AuthMethod,
                // #38 ClientSecret, ClientSecretExpiresAt
                ClientUri = addendum?.ClientUri,
                Contacts = client.Contacts,
                CreatedAt = client.CreatedAt,
                Identifier = client.Identifier,
                GrantTypes = addendum?.GrantTypes,
                Jwks = addendum?.Jwks,
                JwksUri = addendum?.JwksUri,
                LogoUri = addendum?.LogoUri,
                Name = client.Name,
                PolicyUri = addendum?.PolicyUri,
                RedirectUris = [.. client.RedirectUris],
                ResponseTypes = addendum?.ResponseTypes,
                Scope = new ScopeParameter([.. client.Scope]),
                SoftwareId = addendum?.SoftwareId,
                SoftwareVersion = addendum?.SoftwareVersion,
                TosUri = addendum?.TosUri,
                Type = client.Type,
                UpdatedAt = client.UpdatedAt,
                AccessTokenLifetime = client.AccessTokenLifetime,
                RefreshTokenLifetime = client.RefreshTokenLifetime,
                IdTokenLifetime = client.IdTokenLifetime
            };
        }

        private Task<ClientDb?> GetClientDbAsync(string identifier, CancellationToken cancellationToken = default)
        {
            return _context.Clients.FirstOrDefaultAsync(x => x.Identifier.Equals(identifier), cancellationToken);
        }

        public async Task<Result> StoreAsync(Client client, CancellationToken cancellationToken = default)
        {
            ClientDb? dbEntity = await GetClientDbAsync(client.Identifier, cancellationToken);
            if (dbEntity == null)
            {
                dbEntity = new ClientDb
                {
                    Identifier = client.Identifier,
                    Active = client.Active,
                    CreatedAt = client.CreatedAt,
                    CreatedBy = "admin"
                };

                _context.Attach(dbEntity);
            }
            SerializedData addendum = new SerializedData()
            {
                Type = typeof(IClientAddendum).AssemblyQualifiedName!,
                Data = JsonSerializer.Serialize<IClientAddendum>(client)
            };

            dbEntity.AccessTokenLifetime = client.AccessTokenLifetime;
            dbEntity.ClientAddendum = addendum;
            // #38 ClientSecret, ClientSecretExpiresAt
            dbEntity.Contacts = client.Contacts;
            dbEntity.IdTokenLifetime = client.IdTokenLifetime;
            dbEntity.Name = client.Name;
            dbEntity.RedirectUris = [.. client.RedirectUris];
            dbEntity.RefreshTokenLifetime = client.RefreshTokenLifetime;
            dbEntity.Scope = client.Scope.ScopeList;
            dbEntity.Type = client.Type;

            dbEntity.UpdatedAt = client.UpdatedAt;
            dbEntity.UpdatedBy = "admin";

            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
