namespace Sven.Configurations
{
    public static class Modes
    {
        private const string _plain = "plain";
        private const string _pkce = "pkce";

        public static class ExternalLogin
        {
            public const string Pkce = _pkce;
            public const string Plain = _plain;
        }

        public static class Connect
        {
            public static class SignIn
            {
                public const string Pkce = _pkce;
                public const string Plain = _plain;
                public const string Ticket = "ticket";
            }
        }
    }
}
