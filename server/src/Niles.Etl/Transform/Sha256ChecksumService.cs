using System.Security.Cryptography;
using System.Text;

namespace Niles.Etl.Transform
{
    public sealed class Sha256ChecksumService : IChecksumService
    {
        public string ComputeSha256(string filePath)
        {
            using FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using SHA256 sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(fs);
            StringBuilder sb = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++) sb.Append(hash[i].ToString("X2"));
            return sb.ToString();
        }
    }
}
