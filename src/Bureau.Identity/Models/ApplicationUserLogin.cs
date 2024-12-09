using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Bureau.Identity.Models
{
    internal class ApplicationUserLogin : IdentityUserLogin<string>
    {
        /// <summary>
        /// Gets or sets the email address for this user.
        /// </summary>
        [ProtectedPersonalData]
        [MaxLength(256)]
        public string? Email { get; set; }
    }
}
