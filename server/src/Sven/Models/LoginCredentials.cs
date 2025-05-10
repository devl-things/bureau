using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using System.ComponentModel.DataAnnotations;

namespace Sven.Models
{
    public class LoginCredentials
    {
        [FromForm(Name = AuthConstants.OAuth.FieldNames.Username)]
        [Required]
        public string Username { get; set; } = string.Empty;
        [FromForm(Name = AuthConstants.OAuth.FieldNames.Password)]
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
