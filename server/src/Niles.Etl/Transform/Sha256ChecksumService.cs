using Niles.IO;
using System.Security.Cryptography;

namespace Niles.Etl.Transform
{
    public sealed class Sha256ChecksumService : IChecksumService
    {
        public string ComputeHash(string filePath)
        {
            byte[] hash;
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (SHA256 sha = SHA256.Create())
            {
                hash = sha.ComputeHash(fs);
            }

            return Convert.ToHexString(hash);
        }
        public string ComputeHash(byte[] data)
        {
            byte[] hash;
            using (SHA256 sha = SHA256.Create())
            {
                hash = sha.ComputeHash(data);
            }

            return Convert.ToHexString(hash);
        }

        public async Task<string> ComputeHashAsync(Stream data, CancellationToken cancellationToken = default)
        {
            byte[] hash;
            using (SHA256 sha = SHA256.Create())
            {
                hash = await sha.ComputeHashAsync(data, cancellationToken);
            }

            return Convert.ToHexString(hash);
        }
    }
}
