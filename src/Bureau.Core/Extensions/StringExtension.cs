namespace Bureau.Core.Extensions
{
    public static class StringExtension
    {
        public static string Format(this string str, params object?[] args)
        {
            return string.Format(str, args);
        }
    }
}
