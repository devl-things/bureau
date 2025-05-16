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

        public static string? BuildQueryString(Dictionary<string, string> parameters)
        {
            NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);
            foreach (KeyValuePair<string, string> param in parameters)
            {
                query[param.Key] = param.Value;
            }
            return query.ToString();
        }
    }
}
