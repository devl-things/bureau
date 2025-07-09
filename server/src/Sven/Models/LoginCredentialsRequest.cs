using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using System.ComponentModel.DataAnnotations;

namespace Sven.Models
{
    public class LoginCredentialsRequest
    {
        [FromForm(Name = ViewConstants.PropertyNames.Username)]
        [Required]
        public string Username { get; set; } = string.Empty;
        [FromForm(Name = ViewConstants.PropertyNames.Password)]
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
