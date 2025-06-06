using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using System.ComponentModel.DataAnnotations;

namespace Sven.Models
{
    public class LoginCredentialsRequest
    {
        [FromForm(Name = AuthConstants.PropertyNames.Username)]
        [Required]
        public string Username { get; set; } = string.Empty;
        [FromForm(Name = AuthConstants.PropertyNames.Password)]
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
