using System.Security.Claims;

namespace Sven.Models
{
    public class ClientClaims
    {
        public string ClientId { get; set; } = string.Empty;
        public List<Claim> Claims { get; set; } = new List<Claim>();
        public string Scope { get; set; } = string.Empty;

        private HashSet<string>? _scopes;

        public bool HasScope(string scope)
        {
            if (string.IsNullOrWhiteSpace(scope)) return false;

            _scopes ??= Scope
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.ToLowerInvariant())
                .ToHashSet();

            return _scopes.Contains(scope);
        }
    }
}
