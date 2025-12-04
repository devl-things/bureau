using System.Text;

namespace Niles.Chores.Api.Utilities;

internal static class IdObfuscator
{
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static string Encode(int value)
    {
        if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        if (value == 0) return Alphabet[0].ToString();
        StringBuilder sb = new();
        int baseLen = Alphabet.Length;
        while (value > 0)
        {
            int rem = value % baseLen;
            sb.Insert(0, Alphabet[rem]);
            value /= baseLen;
        }
        return sb.ToString();
    }

    public static bool TryDecode(string s, out int value)
    {
        value = 0;
        if (string.IsNullOrEmpty(s)) return false;
        int baseLen = Alphabet.Length;
        foreach (char c in s)
        {
            int idx = Alphabet.IndexOf(c);
            if (idx < 0) return false;
            // check overflow
            if (value > (int.MaxValue - idx) / baseLen) return false;
            value = value * baseLen + idx;
        }
        return true;
    }
}
