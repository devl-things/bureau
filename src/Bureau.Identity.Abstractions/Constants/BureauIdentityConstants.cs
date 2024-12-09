namespace Bureau.Identity.Constants
{
    public static class BureauIdentityConstants
    {
        /// <summary>
        /// Microsoft has just hard coded string in OpenIdConnectHandler
        /// https://github.com/dotnet/aspnetcore/blob/562d119ca4a4275359f6fae359120a2459cd39e9/src/Security/Authentication/OpenIdConnect/src/OpenIdConnectHandler.cs#L940
        /// </summary>
        public const string ExpiresAtTokenName = "expires_at";

        #region google refresh token fields
        /// <summary>
        /// Google auth
        /// https://github.com/googleapis/google-api-dotnet-client/blob/main/Src/Support/Google.Apis.Auth.AspNetCore3/GoogleAuthProvider.cs#L80
        /// </summary>
        public const string ClientIdPropertyName = "client_id";
        public const string ClientSecretPropertyName = "client_secret";
        public const string GrantTypePropertyName = "grant_type";
        public const string GrantTypeRefreshTokenPropertyValue = "refresh_token";
        public const string RefreshTokenPropertyName = "refresh_token";

        public const string ExpiresInPropertyName = "expires_in";
        public const string AccessTokenPropertyName = "access_token";
        #endregion


    }
}
