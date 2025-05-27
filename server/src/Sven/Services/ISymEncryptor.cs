using Bureau.Core;

namespace Sven.Services
{
    public interface ISymEncryptor
    {
        public Result<string> Encrypt(string[] values);
        public Result<string[]> DecryptStringArray(string encrypted);
    }
}
