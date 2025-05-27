using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Sven.Services
{
    public static class SvenValidators
    {
        public static ValidationResult? ValidateEmailDomain(string email, ValidationContext context)
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!regex.IsMatch(email))
            {
                return new ValidationResult("Domain must include a period (e.g., example.com).");
            }
            return ValidationResult.Success;
        }
    }
}
