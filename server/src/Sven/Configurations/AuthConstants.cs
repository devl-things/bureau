namespace Sven.Configurations
{
    public static class AuthConstants
    {

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
