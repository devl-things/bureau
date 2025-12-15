using Bureau.Core;
using Sven.Abstractions.Models;

namespace Sven.Models
{
    public class SvenUser : User
    {
        public string SubjectId { get; set; } = Guid.NewGuid().ToString("N");
        public string PasswordHash { get; set; } = string.Empty;

        public List<LinkedIdentity>? LinkedIdentities { get; set; } = new List<LinkedIdentity>()
        {
            new LinkedIdentity
            {
                ProviderName = "Google",
                Email = "sdfsdf@dssf",
                Status = null
            },
            new LinkedIdentity
            {
                ProviderName = "Microsoft",
                Email = "sdfsdf@dssf",
                Status = true
            },
            new LinkedIdentity
            {
                ProviderName = "Github",
                Email = "sdfssdfsdf@dssf",
                Status = false
            },
        };
    }
}
