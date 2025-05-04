using Bureau.Core;
using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Models;
using Sven.Models;

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
                return new ResultError("Key cannot be empty");
            }
            ClientDb? client = await GetClientDbAsync(clientId, cancellationToken);
            if (client == null)
            {
                return new ResultError("Client doesn't exist.");
            }
            return ToClient(client);
        }

        private static Client ToClient(ClientDb client)
        {
            return new Client()
            {
                Active = client.Active,
                AuthMethod = client.AuthMethod,
                // #38 ClientSecret, ClientSecretExpiresAt
                ClientUri = client.ClientUri,
                Contacts = client.Contacts,
                CreatedAt = client.CreatedAt,
                Identifier = client.Identifier,
                GrantTypes = client.GrantTypes,
                Jwks = client.Jwks,
                JwksUri = client.JwksUri,
                LogoUri = client.LogoUri,
                Name = client.Name,
                PolicyUri = client.PolicyUri,
                RedirectUris = [.. client.RedirectUris],
                ResponseTypes = client.ResponseTypes,
                Scope = new ScopeParameter([.. client.Scope]),
                SoftwareId = client.SoftwareId,
                SoftwareVersion = client.SoftwareVersion,
                TosUri = client.TosUri,
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

            dbEntity.AccessTokenLifetime = client.AccessTokenLifetime;
            dbEntity.AuthMethod = client.AuthMethod;
            // #38 ClientSecret, ClientSecretExpiresAt
            dbEntity.ClientUri = client.ClientUri;
            dbEntity.Contacts = client.Contacts;
            dbEntity.GrantTypes = client.GrantTypes;
            dbEntity.IdTokenLifetime = client.IdTokenLifetime;
            dbEntity.Jwks = client.Jwks;
            dbEntity.JwksUri = client.JwksUri;
            dbEntity.LogoUri = client.LogoUri;
            dbEntity.Name = client.Name;
            dbEntity.PolicyUri = client.PolicyUri;
            dbEntity.RedirectUris = [.. client.RedirectUris];
            dbEntity.RefreshTokenLifetime = client.RefreshTokenLifetime;
            dbEntity.ResponseTypes = client.ResponseTypes;
            dbEntity.Scope = client.Scope.ScopeList;
            dbEntity.SoftwareId = client.SoftwareId;
            dbEntity.SoftwareVersion = client.SoftwareVersion;
            dbEntity.TosUri = client.TosUri;
            dbEntity.Type = client.Type;

            dbEntity.UpdatedAt = client.UpdatedAt;
            dbEntity.UpdatedBy = "admin";

            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
