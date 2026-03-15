using Microsoft.Extensions.Primitives;

namespace Sven.Extensions
{
    public static class HttpRequestExtensions
    {
        public static bool TryGetCookieValue(this HttpRequest request, string key, out string? cookieValue)
        {
            return request.Cookies.TryGetValue(key, out cookieValue) && !string.IsNullOrWhiteSpace(cookieValue);
        }

        public static string? GetQueryStringParameter(this HttpRequest request, string parameterName)
        {
            return request.Query.TryGetValue(parameterName, out StringValues values) ? values.FirstOrDefault() : null;
        }

        public static string? GetFormStringParameter(this HttpRequest request, string parameterName)
        {
            return request.Form.TryGetValue(parameterName, out StringValues values) ? values.FirstOrDefault() : null;
        }
    }
}
