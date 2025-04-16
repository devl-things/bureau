using Microsoft.Extensions.Primitives;

namespace Sven.Extensions
{
    public static class HttpRequestExtension
    {
        public static bool TryGetCookieValue(this HttpRequest request, string key, out string? cookieValue)
        {
            return request.Cookies.TryGetValue(key, out cookieValue) && !string.IsNullOrWhiteSpace(cookieValue);
        }

        public static bool TryGetFormValue(this HttpRequest request, string key, out string? formValue)
        {
            bool exists = request.Form.TryGetValue(key, out StringValues value);
            formValue = value.ToString();
            return exists && !string.IsNullOrWhiteSpace(formValue);
        }
    }
}
