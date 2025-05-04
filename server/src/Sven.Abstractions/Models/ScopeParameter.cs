namespace Sven.Models
{
    public class ScopeParameter
    {
        public ScopeParameter() { }
        public ScopeParameter(string? scope)
        {
            if (!string.IsNullOrWhiteSpace(scope))
            {
                Scope = scope;
            }
        }

        public ScopeParameter(HashSet<string> scopes)
        {
            _scopes = scopes;
            _scope = string.Join(' ', scopes);
        }

        private string _scope = string.Empty;

        public string? Scope
        {
            get { return _scope; }
            set
            {
                _scope = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
                _scopes = [.. Scope!.SplitScope()];
            }
        }

        private HashSet<string>? _scopes;

        /// <summary>
        /// Scope represented as list of strings
        /// </summary>
        public List<string> ScopeList
        {
            get
            {
                return _scopes == null ? [] : [.. _scopes];
            }
        }

        public bool HasScope(string scope)
        {
            if (string.IsNullOrWhiteSpace(scope) || string.IsNullOrWhiteSpace(Scope)) return false;
            return _scopes!.Contains(scope);
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
            return !scope.SplitScope().Any(x => !HasScope(x));
        }

        public string Intersect(string askedScope)
        {
            return string.Join(' ', _scopes!.Intersect(askedScope.SplitScope()));
        }


    }
    internal static class ScopeStringExtension
    {
        public static IEnumerable<string> SplitScope(this string scope)
        {
            return scope.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => s.ToLowerInvariant());
        }
    }
}
