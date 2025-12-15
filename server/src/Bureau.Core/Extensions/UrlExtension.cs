namespace Bureau.Core.Extensions
{
    public static class UrlExtension
    {
        /// <summary>
        /// Returns a canonical representation of the given URL by stripping query
        /// parameters and fragments, while keeping scheme, host (and port), and path.
        /// Useful for deduplication when tokens or transient query values differ.
        /// </summary>
        /// <param name="url">The URL string to canonicalize.</param>
        /// <returns>The canonicalized URL (scheme + host[:port] + absolute path).</returns>
        public static string ToCanonicalUrl(this string url)
        {
            Uri uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Authority}{uri.AbsolutePath}";
        }

        /// <summary>
        /// Extracts the scope identifier from a URL, typically using the host (and port).
        /// </summary>
        /// <param name="url">The base URL to extract the scope from.</param>
        /// <returns>The host (and port) of the URL, or "default" if parsing fails.</returns>
        public static string ToScope(this string url)
        {
            try
            {
                Uri uri = new Uri(url);
                return uri.Authority;
            }
            catch (UriFormatException)
            {
                return "default";
            }
        }
    }
}
