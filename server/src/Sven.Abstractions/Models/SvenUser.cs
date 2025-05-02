using Bureau.Core;

namespace Sven.Models
{
    public class SvenUser : User
    {
        public string SubjectId { get; set; } = Guid.NewGuid().ToString("N");
        public string PasswordHash { get; set; } = string.Empty;

    }
}
