namespace Sven.Extensions
{
    public static class HttpRequestExtension
    {
        public static bool TryGetCookieValue(this HttpRequest request, string key, out string? cookieValue)
        {
            return request.Cookies.TryGetValue(key, out cookieValue) && !string.IsNullOrWhiteSpace(cookieValue);
        }
    }
}
