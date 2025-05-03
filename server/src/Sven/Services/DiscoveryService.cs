using Microsoft.Extensions.Options;
using Sven.Configurations;
using Sven.Models;

namespace Sven.Services
{
    public class DiscoveryService
    {
        private readonly DiscoveryDocument _document;

        private static HashSet<string> _responseTypesSupported = new HashSet<string> { AuthConstants.OAuth.ResponseTypes.Code };
        private static HashSet<string> _grantTypesSupported = new HashSet<string> { AuthConstants.OAuth.GrantTypes.AuthorizationCode, AuthConstants.OAuth.GrantTypes.RefreshToken };
        private static HashSet<string> _subjectTypesSupported = new HashSet<string> { AuthConstants.OAuth.SubjectTypes.Public };
        private static HashSet<string> _idTokenSigningAlgValuesSupported = new HashSet<string> { AuthConstants.OAuth.SigningAlgorithms.Rsa256 };
        private static HashSet<string> _tokenEndpointAuthMethodsSupported = new HashSet<string> { AuthConstants.OAuth.TokenAuthMethods.None };
        private static HashSet<string> _codeChallengeMethodsSupported = new HashSet<string> { AuthConstants.OAuth.CodeChallengeMethods.Sha256 };
        private static HashSet<string> _scopesSupported = new HashSet<string> {
            AuthConstants.Scopes.OpenId, AuthConstants.Scopes.Profile, AuthConstants.Scopes.Email,
            AuthConstants.Scopes.Phone, AuthConstants.Scopes.Address, AuthConstants.Scopes.OfflineAccess
        };
        private static ScopeParameter _scopeSupported = new ScopeParameter(_scopesSupported);
        public DiscoveryService(IOptions<JwtOptions> jwtOptions)
        {
            JwtOptions _jwtOptions = jwtOptions.Value;
            _document = new DiscoveryDocument()
            {
                Issuer = _jwtOptions.Issuer,
                AuthorizationEndpoint = $"{_jwtOptions.Issuer}{Endpoints.Connect.Authorize}",
                TokenEndpoint = $"{_jwtOptions.Issuer}{Endpoints.Connect.Token}",
                UserInfoEndpoint = $"{_jwtOptions.Issuer}{Endpoints.Oidc.UserInfo}",
                JwksUri = $"{_jwtOptions.Issuer}{Endpoints.WellKnown.Jwks}",

                ResponseTypesSupported = _responseTypesSupported.ToList(),
                SubjectTypesSupported = _subjectTypesSupported.ToList(),
                IdTokenSigningAlgValuesSupported = _idTokenSigningAlgValuesSupported.ToList(),
                TokenEndpointAuthMethodsSupported = _tokenEndpointAuthMethodsSupported.ToList(),
                CodeChallengeMethodsSupported = _codeChallengeMethodsSupported.ToList(),
                ScopesSupported = _scopesSupported.ToList(),
            };
        }
        public DiscoveryDocument GetDiscoveryDocument()
        {
            return _document;
        }
        internal bool TryGetSupportedAuthMethod(string? tokenEndpointAuthMethod, out string? authMethod)
        {
            authMethod = AuthConstants.OAuth.TokenAuthMethods.None;
            if (string.IsNullOrWhiteSpace(tokenEndpointAuthMethod) || AuthConstants.OAuth.TokenAuthMethods.None.Equals(tokenEndpointAuthMethod))
            {
                return true;
            }
            return false;
        }
        internal bool TryGetSupportedScope(string? askedScope, out string? supportedScope)
        {
            supportedScope = AuthConstants.Scopes.OpenId;
            if (string.IsNullOrWhiteSpace(askedScope))
            {
                return true;
            }
            supportedScope = _scopeSupported.Intercept(askedScope);
            return !string.IsNullOrWhiteSpace(supportedScope);
        }

        internal bool TryGetSupportedGrantTypes(List<string>? askedGrantTypes, out List<string>? supportedGrantTypes)
        {
            return CopyOrIntercept(_grantTypesSupported, askedGrantTypes, out supportedGrantTypes);
        }

        internal bool TryGetSupportedResponseTypes(List<string>? askedResponseTypes, out List<string>? supportedResponseTypes)
        {
            return CopyOrIntercept(_responseTypesSupported, askedResponseTypes, out supportedResponseTypes);
        }

        private bool CopyOrIntercept(HashSet<string> registry, List<string>? asked, out List<string>? supported)
        {
            if (asked == null || asked.Count == 0)
            {
                supported = registry.ToList();
                return true;
            }
            supported = new List<string>();
            foreach (string item in asked)
            {
                if (registry.Contains(item))
                {
                    supported.Add(item);
                }
            }
            return supported.Count > 0;
        }
    }
}
