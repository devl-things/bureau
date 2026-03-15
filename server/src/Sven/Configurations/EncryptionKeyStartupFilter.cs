using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace Sven.Configurations
{
    internal sealed class EncryptionKeyStartupFilter : IStartupFilter
    {
        private readonly IConfiguration _configuration;

        public EncryptionKeyStartupFilter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            string? key = _configuration["Encrypt:SymKey"];
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new InvalidOperationException(
                    "Encrypt:SymKey is missing. Set via dotnet user-secrets (dev) or ENCRYPT__SYMKEY env var (prod).");
            }

            byte[] keyBytes;
            try
            {
                keyBytes = Convert.FromBase64String(key);
            }
            catch (FormatException)
            {
                throw new InvalidOperationException(
                    "Encrypt:SymKey is not a valid Base64 string.");
            }

            if (keyBytes.Length != 32)
            {
                throw new InvalidOperationException(
                    "Encrypt:SymKey must be a 256-bit (32-byte) Base64 value.");
            }

            return next;
        }
    }
}
