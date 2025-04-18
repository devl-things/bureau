namespace Sven.Configurations
{
    public static class AuthConstants
    {
        public static class Scopes
        {
            public const string OpenId = "openid";
            public const string Profile = "profile";
            public const string Email = "email";
            public const string Address = "address";
            public const string Phone = "phone";
            public const string OfflineAccess = "offline_access";
        }
        public static class OAuth
        {
            public static class ResponseTypes
            {
                public const string Code = "code";
            }

            public static class CodeChallengeMethods
            {
                public const string Sha256 = "S256";
            }

            public static class SigningAlgorithms
            {
                public const string RSA = "RSA";
                public const string Rsa256 = "RS256";
            }
            public static class SubjectTypes
            {
                public const string Public = "public";
            }
            public static class TokenAuthMethods
            {
                public const string None = "none";
            }
            public static class GrantTypes
            {
                public const string AuthorizationCode = "authorization_code";
                public const string RefreshToken = "refresh_token";
            }

            public static class FieldNames
            {
                public const string RedirectUri = "redirect_uri";
                public const string CodeChallenge = "code_challenge";
                public const string CodeChallengeMethod = "code_challenge_method";
                public const string State = "state";
                public const string Scope = "scope";
                public const string ResponseTypeField = "response_type";

                public const string GrantTypeField = "grant_type";
                public const string ClientId = "client_id";
                public const string Code = "code";
                public const string CodeVerifier = "code_verifier";

                public const string IdToken = "id_token";
                public const string RefreshToken = "refresh_token";
                public const string AccessToken = "access_token";
                public const string TokenType = "token_type";
                public const string ExpiresIn = "expires_in";
                public const string Nonce = "nonce";

                //Discovery field names
                public const string Issuer = "issuer";
                public const string AuthorizationEndpoint = "authorization_endpoint";
                public const string TokenEndpoint = "token_endpoint";
                public const string UserInfoEndpoint = "userinfo_endpoint";
                public const string JwksUri = "jwks_uri";

                public const string ResponseTypesSupported = "response_types_supported";
                public const string SubjectTypesSupported = "subject_types_supported";
                public const string IdTokenSigningAlgValuesSupported = "id_token_signing_alg_values_supported";
                public const string TokenEndpointAuthMethodsSupported = "token_endpoint_auth_methods_supported";
                public const string CodeChallengeMethodsSupported = "code_challenge_methods_supported";
                public const string ScopesSupported = "scopes_supported";

                //Errors
                public const string Error = "error";
                public const string ErrorDescription = "error_description";
                public const string ErrorUri = "error_uri";


            }
            public static class Errors
            {
                // Standard OAuth 2.0 errors
                public const string InvalidRequest = "invalid_request";
                public const string InvalidClient = "invalid_client";
                public const string InvalidGrant = "invalid_grant";
                public const string UnauthorizedClient = "unauthorized_client";
                public const string UnsupportedGrantType = "unsupported_grant_type";
                public const string InvalidScope = "invalid_scope";
                public const string AccessDenied = "access_denied";
                public const string ServerError = "server_error";
                public const string TemporarilyUnavailable = "temporarily_unavailable";
                public const string UnsupportedResponseType = "unsupported_response_type";

                // OpenID Connect-specific errors
                public const string InteractionRequired = "interaction_required";
                public const string LoginRequired = "login_required";
                public const string AccountSelectionRequired = "account_selection_required";
                public const string ConsentRequired = "consent_required";

                // Errors returned from authorization endpoint with redirect
                public const string RequestNotSupported = "request_not_supported";
                public const string RequestUriNotSupported = "request_uri_not_supported";
                public const string RegistrationNotSupported = "registration_not_supported";
            }
        }
        public static class CookieNames
        {
            public const string PkceKey = "pkce_key";
            public const string ClaimsKey = "claims_key";
        }

        public static class ExternalSchemes
        {
            public const string Google = "google";
            public const string Microsoft = "microsoft";

            public static bool Exists(string scheme)
            {
                return Google.Equals(scheme) || Microsoft.Equals(scheme);
            }
        }
    }
}
