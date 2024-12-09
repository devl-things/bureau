using Bureau.Core;
using Bureau.Identity.Models;
using System.Security.Claims;

namespace Bureau.Identity.Providers
{
    public interface IIdentityProvider
    {
        public Task<Result<BureauUser>> GetUserAsync(CancellationToken cancellationToken = default);
        public Result<IUserId> GetUserId(ClaimsPrincipal user);
        public Result<bool> IsUserAuthenticated();
    }
}
