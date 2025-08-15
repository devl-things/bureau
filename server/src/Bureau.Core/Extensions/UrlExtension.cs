namespace Bureau.Core.Extensions
{
    public static class UrlExtension
    {
        // Strip query/fragment so the same file with different tokens counts once.
        public static string CanonicalizeUrl(this string url)
        {
            Uri uri = new Uri(url);
            // keep scheme/host/path; drop query & fragment
            return $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
        }
    }
}
