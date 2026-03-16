namespace Sven.Configurations
{
    public static class Endpoints
    {
        public static class Account
        {
            public const string Base = "account";
            public const string ChangePasswordPath = "change-password";
            public const string LinkedIdentitiesPath = "linked";
            public const string AccountInfo = $"/{Base}";
            public const string ChangePassword = $"/{Base}/{ChangePasswordPath}";
            public const string LinkedIdentities = $"/{Base}/{LinkedIdentitiesPath}";
        }
        public static class Oidc
        {
            public const string Base = "oidc";
            public const string UserInfoPath = "userinfo";
            public const string RegisterPath = "register";
            public const string IntrospectPath = "introspect";

            public const string UserInfo = $"/{Base}/{UserInfoPath}";
            public const string Register = $"/{Base}/{RegisterPath}";
            public const string Introspect = $"/{Base}/{IntrospectPath}";
        }
        public static class WellKnown
        {
            public const string Base = ".well-known";
            public const string OpenConfigurationPath = "openid-configuration";
            public const string JwksPath = "jwks.json";

            public const string OpenConfiguration = $"/{Base}/{OpenConfigurationPath}";
            public const string Jwks = $"/{Base}/{JwksPath}";
        }
        public static class External
        {
            public const string Base = "external";
            public const string SignInBase = "signin";
            public const string SignIn = $"/{Base}/{SignInBase}";
            public const string SignInWithProvider = SignInBase + "/{provider}";


            public const string SignInGoogle = $"/{Base}/{SignInBase}/google";
            public const string SignInMicrosoft = $"/{Base}/{SignInBase}/microsoft";
        }

        public static class Connect
        {
            public const string Base = "connect";
            public const string AuthorizePath = "authorize";
            public const string TokenPath = "token";
            public const string RevocationPath = "revocation";
            public const string SignInPath = "signin";
            public const string SignUpPath = "signup";
            public const string ForgotPasswordPath = "forgot";
            public const string LoginPath = "login";

            public const string Authorize = $"/{Base}/{AuthorizePath}";
            public const string AuthorizePkce = $"{Authorize}/{Modes.ExternalLogin.Pkce}";
            public const string AuthorizePlain = $"{Authorize}/{Modes.ExternalLogin.Plain}";
            public const string Token = $"/{Base}/{TokenPath}";
            public const string Revocation = $"/{Base}/{RevocationPath}";
            public const string SignIn = $"/{Base}/{SignInPath}";
            public const string SignInPkce = $"{SignIn}?{AuthConstants.PropertyNames.Mode}={Modes.Connect.SignIn.Pkce}";
            public const string SignUp = $"/{Base}/{SignUpPath}";
            public const string ForgotPassword = $"/{Base}/{ForgotPasswordPath}";
        }
    }
}
