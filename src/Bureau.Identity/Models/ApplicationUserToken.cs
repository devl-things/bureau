using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Bureau.Identity.Models;

internal class ApplicationUserToken: IdentityUserToken<string>
{
    /// <summary>
    /// Gets or sets the unique provider identifier for this login.
    /// </summary>
    [MaxLength(450)]
    public string ProviderKey { get; set; }
}
