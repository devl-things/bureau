using System.Text.RegularExpressions;

namespace Bureau.Core.Extensions
{
    public static class StringExtension
    {
        public static string Format(this string str, params object?[] args)
        {
            return string.Format(str, args);
        }

        public static string ToPascalCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Step 1: Replace any separators (spaces, underscores, or hyphens) with a space for consistent splitting
            string[] words = Regex.Split(input, @"[\s_\-]+", RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Step 2: Capitalize the first letter of each word and concatenate them
            var pascalCase = string.Concat(words.Select(word =>
                char.ToUpperInvariant(word[0]) + word.Substring(1).ToLowerInvariant()));

            return pascalCase;
        }   

        public static string ToKebabCase(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Step 1: Replace spaces and underscores with a hyphen
            string result = Regex.Replace(input, @"[\s_]+", "-", RegexOptions.None, TimeSpan.FromMilliseconds(200));

            // Step 2: Insert hyphens before uppercase letters (for camelCase or PascalCase)
            result = Regex.Replace(result, @"([a-z0-9])([A-Z])", "$1-$2");

            // Step 3: Convert to lowercase
            result = result.ToLowerInvariant();

            return result;
        }
    }
}
