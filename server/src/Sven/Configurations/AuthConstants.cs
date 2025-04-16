namespace Sven.Configurations
{
    public static class AuthConstants
    {
        public static class Scopes
        {
            public const string OpenId = "openid";
            public const string Profile = "profile";
            public const string Email = "email";
            public const string OfflineAccess = "offline_access";
        }
        public static class OAuth
        {
            public static class ResponseType
            {
                public const string Code = "code";
            }

            public static class CodeChallengeMethods
            {
                public const string Sha256 = "S256";
            }

            public static class GrantType
            {
                public const string AuthorizationCode = "authorization_code";
                public const string RefreshToken = "refresh_token";
            }

            public static class FieldNames
            {
                public const string GrantTypeField = "grant_type";
                public const string ClientId = "client_id";
                public const string Code = "code";
                public const string CodeVerifier = "code_verifier";

                public const string RefreshToken = "refresh_token";
                public const string AccessToken = "access_token";
                public const string TokenType = "token_type";
                public const string ExpiresIn = "expires_in";

                public const string RedirectUri = "redirect_uri";
                public const string Scope = "scope";
                public const string ResponseTypeField = "response_type";
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
