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
        public async Task<Result<Client>> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return new ResultError("Key cannot be empty");
            }
            ClientDb? client = await _context.Clients.Where(x => x.Identifier == key).FirstOrDefaultAsync(cancellationToken);
            if (client == null)
            {
                return new ResultError("Client doesn't exist.");
            }
            Client result = new Client();
            result.Identifier = client.Identifier;
            result.Active = client.Active;
            result.CreatedAt = client.CreatedAt;
            result.UpdatedAt = client.UpdatedAt;
            result.Name = client.Name;
            result.AccessTokenLifetime = client.AccessTokenLifetime;
            result.RefreshTokenLifetime = client.RefreshTokenLifetime;
            result.IdTokenLifetime = client.IdTokenLifetime;
            result.RedirectUris = new HashSet<string>(client.RedirectUris);
            result.Scope = new ScopeParameter(new HashSet<string>(client.Scope));
            result.Type = client.Type;
            return result;
        }

        private Task<ClientDb?> GetClientDbAsync(string identifier, CancellationToken cancellationToken = default)
        {
            return _context.Clients.FirstOrDefaultAsync(x => x.Identifier.Equals(identifier), cancellationToken);
        }

        public async Task<Result> StoreAsync(Client client, CancellationToken cancellationToken)
        {
            ClientDb? dbEntity = await GetClientDbAsync(client.Identifier, cancellationToken);
            if (dbEntity == null)
            {
                dbEntity = new ClientDb();
                dbEntity.Identifier = client.Identifier;
                dbEntity.Active = client.Active;
                dbEntity.CreatedAt = client.CreatedAt;
                dbEntity.CreatedBy = "admin";

                _context.Attach(dbEntity);
            }

            dbEntity.Name = client.Name;

            dbEntity.UpdatedAt = client.UpdatedAt;
            dbEntity.UpdatedBy = "admin";

            dbEntity.AccessTokenLifetime = client.AccessTokenLifetime;
            dbEntity.RefreshTokenLifetime = client.RefreshTokenLifetime;
            dbEntity.IdTokenLifetime = client.IdTokenLifetime;
            dbEntity.RedirectUris = client.RedirectUris.ToList();
            dbEntity.Scope = client.Scope.Scopes.ToList();
            dbEntity.Type = client.Type;
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
