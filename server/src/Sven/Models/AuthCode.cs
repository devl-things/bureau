using Sven.Configurations;
using System.Text;

namespace Sven.Models
{
    public class AuthCode : ClientClaims
    {
        public string Code { get; set; } = string.Empty;
        public string CodeChallenge { get; set; } = string.Empty;
        public string CodeChallengeMethod { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }

        public bool IsCodeVerifierValid(string codeVerifier)
        {
            string hashed = AuthConstants.OAuth.CodeChallengeMethods.Sha256.Equals(CodeChallengeMethod) ?
                GenerateSha256CodeChallenge(codeVerifier) : codeVerifier;

            return CodeChallenge.Equals(hashed);
        }

        public static string GenerateSha256CodeChallenge(string value)
        {
            return Base64CodeEncode(System.Security.Cryptography.SHA256.HashData(Encoding.ASCII.GetBytes(value)));
        }

        private static string Base64CodeEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}
