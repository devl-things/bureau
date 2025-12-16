using Microsoft.AspNetCore.Identity;

namespace Sven.Services
{
    public static class PasswordHasher
    {
        private static readonly PasswordHasher<object> _hasher = new();

        public static string HashPassword(string password)
        {
            return _hasher.HashPassword(null!, password);
        }

        public static bool VerifyPassword(string hashedPassword, string inputPassword)
        {
            PasswordVerificationResult result = _hasher.VerifyHashedPassword(null!, hashedPassword, inputPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}
