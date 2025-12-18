using Bureau;
using Bureau.Primitives.Errors;
using Bureau.Server.Contracts;
using System.Text;

namespace Niles.Chores.Api.Utilities
{
    internal sealed class IdObfuscator : IIdObfuscator
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public string Encode(int value)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            if (value == 0)
            {
                return Alphabet[0].ToString();
            }
            StringBuilder sb = new StringBuilder();
            int baseLen = Alphabet.Length;
            while (value > 0)
            {
                int rem = value % baseLen;
                sb.Insert(0, Alphabet[rem]);
                value /= baseLen;
            }
            return sb.ToString();
        }

        public Result<int> Decode(string? id)
        {
            if (!TryDecode(id, out int intId))
            {
                return ResultError.FromLogMessage(ProblemCodes.Request.InvalidId, string.Format("Id = {0} cannot be decoded", id));
            }
            return intId;
        }

        private static bool TryDecode(string? s, out int value)
        {
            value = 0;
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }
            int baseLen = Alphabet.Length;
            foreach (char c in s)
            {
                int idx = Alphabet.IndexOf(c);
                if (idx < 0)
                {
                    return false;
                }
                // check overflow
                if (value > (int.MaxValue - idx) / baseLen)
                {
                    return false;
                }
                value = value * baseLen + idx;
            }
            return true;
        }
    }
}
