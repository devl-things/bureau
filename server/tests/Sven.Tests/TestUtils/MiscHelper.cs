using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Web;

namespace Sven.Tests.TestUtils
{
    internal static class MiscHelper
    {
        public const string AntiforgeryFormKey = "__RequestVerificationToken";
        public static string GetAntiforgeryTokenFromContent(string content)
        {
            return Regex.Match(content, @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" />").Groups[1].Value;
        }
        public static string GetHiddenInputValue(string content, string inputName)
        {
            if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(inputName))
            {
                return string.Empty;
            }

            string pattern = $@"<input[^>]*type=""hidden""[^>]*name=""{Regex.Escape(inputName)}""[^>]*value=""([^""]+)""[^>]*>";
            Match match = Regex.Match(content, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(20));

            return match.Success ? match.Groups[1].Value : string.Empty;
        }

        public static Dictionary<string, string?> GetQueryParameters(string? url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return new Dictionary<string, string?>();
            }

            Uri uri = new Uri(url);
            NameValueCollection query = HttpUtility.ParseQueryString(uri.Query);

            return query.AllKeys
                .Where(key => key != null)
                .ToDictionary(key => key!, key => query[key]);
        }

        public static string? BuildQueryString(Dictionary<string, string> parameters)
        {
            NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);
            foreach (KeyValuePair<string, string> param in parameters)
            {
                query[param.Key] = param.Value;
            }
            return query.ToString();
        }

        public static string? ExtractWholeUrlStartingWith(string input, string url)
        {
            // Match anything starting with /connect/forgot up to the next whitespace
            var match = Regex.Match(input, @$"({url}\S*)");
            return match.Success ? match.Value : null;
        }
    }
}
