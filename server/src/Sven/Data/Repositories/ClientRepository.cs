using Bureau;
using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Models;
using Sven.Models;
using System.Text.Json;

namespace Sven.Data.Repositories
{
    internal class ClientRepository : IClientRepository
    {
        private readonly SvenContext _context;
        public ClientRepository(SvenContext context)
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
                PostLogoutRedirectUris = client.PostLogoutRedirectUris,
                BureauFeatures = client.ClientFeatures != null && client.ClientFeatures.Count > 0
                    ? client.ClientFeatures.ToDictionary(
                        f => f.FeatureKey,
                        f => f.ExternalRequirements.Select(r => r.ExternalScopeKey).ToList())
                    : null,
                ResponseTypes = addendum?.ResponseTypes,
                Scope = new ScopeParameter([.. client.Scope]),
                SoftwareId = addendum?.SoftwareId,
                SoftwareVersion = addendum?.SoftwareVersion,
                TosUri = addendum?.TosUri,
                Type = client.Type,
                HashedSecret = client.HashedSecret,
                UpdatedAt = client.UpdatedAt,
                AccessTokenLifetime = client.AccessTokenLifetime,
                RefreshTokenLifetime = client.RefreshTokenLifetime,
                IdTokenLifetime = client.IdTokenLifetime
            };
        }

        private Task<ClientDb?> GetClientDbAsync(string identifier, CancellationToken cancellationToken = default)
        {
            return _context.Clients
                .Include(x => x.ClientFeatures)
                    .ThenInclude(f => f.ExternalRequirements)
                .FirstOrDefaultAsync(x => x.Identifier.Equals(identifier), cancellationToken);
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
            dbEntity.Contacts = client.Contacts;
            dbEntity.HashedSecret = client.HashedSecret;
            dbEntity.IdTokenLifetime = client.IdTokenLifetime;
            dbEntity.Name = client.Name;
            dbEntity.RedirectUris = [.. client.RedirectUris];
            dbEntity.PostLogoutRedirectUris = client.PostLogoutRedirectUris;
            dbEntity.RefreshTokenLifetime = client.RefreshTokenLifetime;
            dbEntity.Scope = client.Scope.ScopeList;
            dbEntity.Type = client.Type;

            dbEntity.UpdatedAt = client.UpdatedAt;
            dbEntity.UpdatedBy = "admin";

            await _context.SaveChangesAsync(cancellationToken);

            List<ClientFeatureDb> existingFeatures = await _context.Set<ClientFeatureDb>()
                .Where(f => f.ClientId == dbEntity.Id)
                .Include(f => f.ExternalRequirements)
                .ToListAsync(cancellationToken);
            _context.Set<ClientFeatureDb>().RemoveRange(existingFeatures);

            if (client.BureauFeatures != null)
            {
                foreach (KeyValuePair<string, List<string>> feature in client.BureauFeatures)
                {
                    ClientFeatureDb featureDb = new ClientFeatureDb
                    {
                        ClientId = dbEntity.Id,
                        FeatureKey = feature.Key,
                        ExternalRequirements = feature.Value.Select(s => new ClientFeatureExternalRequirementDb
                        {
                            ClientId = dbEntity.Id,
                            FeatureKey = feature.Key,
                            ExternalScopeKey = s
                        }).ToList()
                    };
                    _context.Set<ClientFeatureDb>().Add(featureDb);
                }
            }
            return await _context.SaveChangesAsync(cancellationToken) >= 0;
        }
    }
}
