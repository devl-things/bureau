namespace Sven.Services
{
    public static class UriValidator
    {
        public static bool IsRedirectUriValid(string redirectUri)
        {
            return Uri.TryCreate(redirectUri, UriKind.Absolute, out Uri? value) && string.IsNullOrWhiteSpace(value.Fragment);
        }
        public static bool IsUriValid(string uri)
        {
            return Uri.IsWellFormedUriString(uri, UriKind.Absolute);
        }
    }
}
