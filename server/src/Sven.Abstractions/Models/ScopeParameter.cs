namespace Sven.Models
{
    public class ScopeParameter
    {
        public ScopeParameter() { }
        public ScopeParameter(string scope)
        {
            Scope = scope;
            SetScopes();
        }

        public ScopeParameter(HashSet<string> scopes)
        {
            _scopes = scopes;
            _scope = string.Join(" ", scopes);
        }

        private string _scope = string.Empty;

        public string? Scope
        {
            get { return _scope; }
            set
            {
                _scope = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
                _scopes = null;
            }
        }

        private HashSet<string>? _scopes;
        public HashSet<string> Scopes
        {
            get
            {
                return _scopes ?? new HashSet<string>(0);
            }
        }

        public bool HasScope(string scope)
        {
            if (string.IsNullOrWhiteSpace(scope)) return false;
            if (_scopes == null && string.IsNullOrWhiteSpace(Scope)) return false;
            SetScopes();
            return _scopes.Contains(scope);
        }

        private void SetScopes()
        {
            _scopes ??= Scope!
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.ToLowerInvariant())
                .ToHashSet();
        }

        /// <summary>
        /// If the scope is empty or null, it will return true (because it's definitely the subset of Scope).
        /// If the Scope is empty or null and scope is not empty or null then returns false;
        /// </summary>
        /// <param name="scope"></param>
        /// <returns></returns>
        public bool IsScopeSameOrSubset(string? scope)
        {
            if (string.IsNullOrWhiteSpace(scope)) return true;
            if (string.IsNullOrWhiteSpace(_scope)) return false;

            foreach (var s in scope.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToLowerInvariant()))
            {
                if (!HasScope(s)) return false;
            }
            return true;
        }
    }
}
