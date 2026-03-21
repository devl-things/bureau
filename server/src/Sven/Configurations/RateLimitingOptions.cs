namespace Sven.Configurations
{
    public sealed class RateLimitingOptions
    {
        public int TokenPermitLimit
        {
            get { return _tokenPermitLimit; }
            set { _tokenPermitLimit = value; }
        }

        public int TokenWindowSeconds
        {
            get { return _tokenWindowSeconds; }
            set { _tokenWindowSeconds = value; }
        }

        public int AuthorizePermitLimit
        {
            get { return _authorizePermitLimit; }
            set { _authorizePermitLimit = value; }
        }

        public int AuthorizeWindowSeconds
        {
            get { return _authorizeWindowSeconds; }
            set { _authorizeWindowSeconds = value; }
        }

        public int SignInPermitLimit
        {
            get { return _signInPermitLimit; }
            set { _signInPermitLimit = value; }
        }

        public int SignInWindowSeconds
        {
            get { return _signInWindowSeconds; }
            set { _signInWindowSeconds = value; }
        }

        private int _tokenPermitLimit = 10;
        private int _tokenWindowSeconds = 60;
        private int _authorizePermitLimit = 20;
        private int _authorizeWindowSeconds = 60;
        private int _signInPermitLimit = 10;
        private int _signInWindowSeconds = 300;
    }
}
