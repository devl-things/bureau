using Microsoft.Extensions.Options;
using Sven.Configurations;
using Sven.Models;

namespace Sven.Services
{
    public class DiscoveryService
    {
        private readonly DiscoveryDocument _document;

        private static readonly HashSet<string> _responseTypesSupported = [AuthConstants.OAuth.ResponseTypes.Code];
        private static readonly HashSet<string> _grantTypesSupported =
        [
            AuthConstants.OAuth.GrantTypes.AuthorizationCode,
            AuthConstants.OAuth.GrantTypes.RefreshToken,
            AuthConstants.OAuth.GrantTypes.ClientCredentials
        ];
        private static readonly HashSet<string> _subjectTypesSupported = [AuthConstants.OAuth.SubjectTypes.Public];
        private static readonly HashSet<string> _idTokenSigningAlgValuesSupported = [AuthConstants.OAuth.SigningAlgorithms.Rsa256];
        private static readonly HashSet<string> _tokenEndpointAuthMethodsSupported =
        [
            AuthConstants.OAuth.TokenAuthMethods.None,
            AuthConstants.OAuth.TokenAuthMethods.ClientSecretBasic,
            AuthConstants.OAuth.TokenAuthMethods.ClientSecretPost
        ];
        private static readonly HashSet<string> _codeChallengeMethodsSupported = [AuthConstants.OAuth.CodeChallengeMethods.Sha256];
        private static readonly HashSet<string> _scopesSupported =
        [
            AuthConstants.Scopes.OpenId, AuthConstants.Scopes.Profile, AuthConstants.Scopes.Email,
            AuthConstants.Scopes.Phone, AuthConstants.Scopes.Address, AuthConstants.Scopes.OfflineAccess
        ];
        public readonly static ScopeParameter ScopeSupported = new(_scopesSupported);
        private readonly static ScopeParameter _scopeDefault = new([AuthConstants.Scopes.Profile]);

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
                IntrospectionEndpoint = $"{_jwtOptions.Issuer}{Endpoints.Oidc.Introspect}",
                EndSessionEndpoint = $"{_jwtOptions.Issuer}{Endpoints.Connect.EndSession}",

                ResponseTypesSupported = [.. _responseTypesSupported],
                SubjectTypesSupported = [.. _subjectTypesSupported],
                IdTokenSigningAlgValuesSupported = [.. _idTokenSigningAlgValuesSupported],
                TokenEndpointAuthMethodsSupported = [.. _tokenEndpointAuthMethodsSupported],
                CodeChallengeMethodsSupported = [.. _codeChallengeMethodsSupported],
                ScopesSupported = [.. _scopesSupported],
            };
        }
        public DiscoveryDocument GetDiscoveryDocument()
        {
            return _document;
        }
        internal static bool TryGetSupportedAuthMethod(string? tokenEndpointAuthMethod, out string? authMethod)
        {
            authMethod = AuthConstants.OAuth.TokenAuthMethods.None;
            if (string.IsNullOrWhiteSpace(tokenEndpointAuthMethod)
                || AuthConstants.OAuth.TokenAuthMethods.None.Equals(tokenEndpointAuthMethod))
            {
                return true;
            }
            if (AuthConstants.OAuth.TokenAuthMethods.ClientSecretBasic.Equals(tokenEndpointAuthMethod)
                || AuthConstants.OAuth.TokenAuthMethods.ClientSecretPost.Equals(tokenEndpointAuthMethod))
            {
                authMethod = tokenEndpointAuthMethod;
                return true;
            }
            return false;
        }
        internal static bool TryGetSupportedScope(string? askedScope, out string? supportedScope)
        {
            supportedScope = _scopeDefault.Scope;
            if (string.IsNullOrWhiteSpace(askedScope))
            {
                return true;
            }
            supportedScope = ScopeSupported.Intersect(askedScope);
            return !string.IsNullOrWhiteSpace(supportedScope);
        }

        internal static bool TryGetSupportedGrantTypes(List<string>? askedGrantTypes, out List<string>? supportedGrantTypes)
        {
            return CopyOrIntersect(_grantTypesSupported, askedGrantTypes, out supportedGrantTypes);
        }

        internal static bool TryGetSupportedResponseTypes(List<string>? askedResponseTypes, out List<string>? supportedResponseTypes)
        {
            return CopyOrIntersect(_responseTypesSupported, askedResponseTypes, out supportedResponseTypes);
        }

        private static bool CopyOrIntersect(HashSet<string> registry, List<string>? asked, out List<string>? supported)
        {
            if (asked == null || asked.Count == 0)
            {
                supported = [.. registry];
                return true;
            }
            supported = [.. registry.Intersect(asked)];
            return supported.Count > 0;
        }
    }
}
