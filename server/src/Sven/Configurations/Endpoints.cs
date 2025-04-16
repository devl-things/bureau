namespace Sven.Configurations
{
    public static class Endpoints
    {
        public static class External
        {
            public const string Base = "external";
            public const string SignInBase = "signin";
            public const string SignInWithProvider = SignInBase + "/{provider}";

            public const string SignInGoogle = $"/{Base}/{SignInBase}/google";
            public const string SignInMicrosoft = $"/{Base}/{SignInBase}/microsoft";
        }

        public static class Connect
        {
            public const string Base = "connect";
            public const string AuthorizePath = "authorize";
            public const string AuthorizeContinuePath = "authorize/continue";
            public const string AuthorizeLoginPath = "authorize/login";
            public const string TokenPath = "token";

            public const string AuthorizePage = "/connect/authorize/page";
            public const string Authorize = $"/{Base}/{AuthorizePath}";
            public const string AuthorizeContinue = $"/{Base}/{AuthorizeContinuePath}";
            public const string AuthorizeLogin = $"/{Base}/{AuthorizeLoginPath}";
            public const string Token = $"/{Base}/{TokenPath}";


        }
    }
}
