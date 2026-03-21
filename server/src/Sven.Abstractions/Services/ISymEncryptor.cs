using Bureau;

namespace Sven.Services
{
    public interface ISymEncryptor
    {
        public Result<string> Encrypt(string[] values);
        public Result<string> Encrypt(string value);
        public Result<string[]> DecryptStringArray(string encrypted);
        public Result<string> DecryptString(string encrypted);
    }
}
