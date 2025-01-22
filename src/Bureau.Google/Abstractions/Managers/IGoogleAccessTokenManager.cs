using Bureau.Core;
using Bureau.Identity.Models;
using System.Net.Http.Headers;

namespace Bureau.Google.Managers
{
    internal interface IGoogleAccessTokenManager
    {
        public Task<Result<AuthenticationHeaderValue>> GetAuthenticationHeaderAsync(BureauUserLoginModel userLogin, CancellationToken cancellationToken = default);
    }
}
