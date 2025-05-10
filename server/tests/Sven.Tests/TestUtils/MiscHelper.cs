using System.Text.RegularExpressions;

namespace Sven.Tests.TestUtils
{
    internal static class MiscHelper
    {
        public const string AntiforgeryFormKey = "__RequestVerificationToken";
        public static string GetAntiforgeryTokenFromContent(string content)
        {
            return Regex.Match(content, @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""([^""]+)"" />").Groups[1].Value;
        }

    }
}
