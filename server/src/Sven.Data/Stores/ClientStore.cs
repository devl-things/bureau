using Bureau.Core;
using Microsoft.EntityFrameworkCore;
using Sven.Abstractions.Services;
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
            ClientDb? client = await _context.Clients.Where(x => x.Identifier == key).FirstOrDefaultAsync();
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
            result.RedirectUris = client.RedirectUris;
            result.Scope = new ScopeParameter(client.Scope);
            result.Type = client.Type;
            return result;
        }
    }
}
