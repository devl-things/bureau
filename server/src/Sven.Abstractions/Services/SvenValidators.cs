using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Sven.Services
{
    public static class SvenValidators
    {
        /// <summary>
        /// Checks if <para>email</para> is not compliant with RFC5322
        /// Doesn't allow % in local part of email address
        /// </summary>
        /// <param name="email"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static ValidationResult? ValidateEmailDomain(string email, ValidationContext context)
        {
            var regex = new Regex(@"^[a-zA-Z0-9._+-]+@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.[a-zA-Z]{2,}$", RegexOptions.None, TimeSpan.FromMilliseconds(100));
            if (!regex.IsMatch(email))
            {
                return new ValidationResult("Email malformed, well formed emails are alike this-is@example.com");
            }
            return ValidationResult.Success;
        }
    }
}
