using Bureau.Core;
using System.Security.Claims;

namespace Sven.Services
{
    public interface IUserClaimsProvider
    {
        Task<Result<ClaimsPrincipal>> GetClaimsPrincipalAsync(string username, string password, CancellationToken cancellationToken = default);
    }
}
