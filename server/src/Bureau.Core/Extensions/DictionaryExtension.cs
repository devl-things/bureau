namespace Bureau.Core.Extensions
{
    public static class DictionaryExtension
    {
        /// <summary>
        /// Gets the value associated with the specified <paramref name="key"/> if it exists and is not null (or
        /// whitespace in the case of <see cref="string"/>).  
        /// Otherwise, returns the provided <paramref name="fallback"/>.
        /// </summary>
        /// <typeparam name="TKey">The type of the dictionary key.</typeparam>
        /// <typeparam name="TValue">The type of the dictionary value.</typeparam>
        /// <param name="dictionary">The dictionary to search.</param>
        /// <param name="key">The key to locate in the dictionary.</param>
        /// <param name="fallback">The value to return if the key is not found or the value is null/whitespace (for strings).</param>
        /// <returns>
        /// The value if found and not null/whitespace (for strings); otherwise the fallback.
        /// </returns>
        public static TValue GetNotNullOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue fallback)
        {
            if (dictionary == null)
            {
                return fallback;
            }

            if (dictionary.TryGetValue(key, out TValue? value))
            {
                // Special handling for strings: exclude null/whitespace
                if (value is string s && string.IsNullOrWhiteSpace(s))
                {
                    return fallback;
                }

                if (value != null)
                {
                    return value;
                }
            }

            return fallback;
        }

        /// <summary>
        /// Attempts to parse a boolean value from a dictionary entry, 
        /// supporting common string representations (true/false, 1/0, yes/no, on/off).
        /// </summary>
        public static bool TryGetBool(this IDictionary<string, string> args, string key, out bool result)
        {
            result = false;

            if (!args.TryGetValue(key, out string? raw) || string.IsNullOrWhiteSpace(raw))
                return false;

            string s = raw.Trim();

            if (bool.TryParse(s, out result))
                return true;

            switch (s.ToLowerInvariant())
            {
                case "1":
                case "yes":
                case "y":
                case "on":
                    result = true;
                    return true;

                case "0":
                case "no":
                case "n":
                case "off":
                    result = false;
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Attempts to parse an integer value from a dictionary entry,
        /// supporting decimal, hexadecimal (0x prefix), and numeric string representations.
        /// </summary>
        public static bool TryGetInt(this IDictionary<string, string> args, string key, out int result)
        {
            result = 0;

            if (!args.TryGetValue(key, out string? raw) || string.IsNullOrWhiteSpace(raw))
                return false;

            string s = raw.Trim();

            // Try decimal/standard parse
            if (int.TryParse(s, out result))
                return true;

            // Try hex (0x prefix)
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(s.Substring(2), System.Globalization.NumberStyles.HexNumber,
                             System.Globalization.CultureInfo.InvariantCulture, out result))
            {
                return true;
            }

            return false;
        }
    }
}
