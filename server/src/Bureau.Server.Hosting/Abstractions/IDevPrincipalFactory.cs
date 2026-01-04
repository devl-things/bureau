using System.Security.Claims;

namespace Bureau.Server.Hosting
{
    public interface IDevPrincipalFactory
    {
        ClaimsPrincipal CreatePrincipal(DevAuthOptions options);
    }
}
