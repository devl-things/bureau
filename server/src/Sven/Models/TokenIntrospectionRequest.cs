using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using System.ComponentModel.DataAnnotations;

namespace Sven.Models
{
    public class TokenIntrospectionRequest
    {
        [FromForm(Name = AuthConstants.OAuth.FieldNames.Token)]
        [Required]
        public string Token { get; set; } = string.Empty;

        [FromForm(Name = AuthConstants.OAuth.FieldNames.ClientId)]
        public string? ClientId { get; set; }

        [FromForm(Name = AuthConstants.OAuth.FieldNames.TokenTypeHint)]
        public string? TokenTypeHint { get; set; }
    }
}
