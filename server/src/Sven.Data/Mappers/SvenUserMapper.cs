using Sven.Data.Models;
using Sven.Models;

namespace Sven.Data.Mappers
{
    internal static class SvenUserMapper
    {
        internal static SvenUser ToSvenUser(this UserDb user)
        {
            return new SvenUser()
            {
                DisplayName = user.DisplayName,
                PasswordHash = user.PasswordHash,
                SubjectId = user.Identifier,
                Username = user.Username
            };
        }
    }
}
