using Bureau.Core;
using Bureau.Core.Factories;
using Bureau.UI.Accessors;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Bureau.UI.Accessors
{
    public class UserAccessor : IUserAccessor
    {
        private readonly Task<AuthenticationState> _authenticationStateTask;
        public UserAccessor(AuthenticationStateProvider authenticationStateProvider) 
        {
            _authenticationStateTask = authenticationStateProvider.GetAuthenticationStateAsync();
        }
        public async Task<IUserId> GetUserIdAsync(CancellationToken cancellationToken = default)
        {
            AuthenticationState authenticationState = await _authenticationStateTask;
            string userId = authenticationState.User.Claims
                .Where(x => x.ValueType == ClaimTypes.NameIdentifier)
                .Select(x => x.Value)
                .FirstOrDefault();
            return BureauUserIdFactory.CreateIBureauUserId(userId);
        }
    }
}
