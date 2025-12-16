using Sven.Models;
using System.Security.Claims;

namespace Sven.Services
{
    public interface ICurrentUserProvider
    {
        public SvenUser CurrentUser { get; }

        public bool IsAuthenticated { get; }

        public Task SetUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken = default);
    }
}
