using Sven.Models;
using System.Security.Claims;

namespace Sven.Services
{
    public interface ITokenProvider
    {
        SvenToken CreateToken(List<Claim> claims);
    }
}
