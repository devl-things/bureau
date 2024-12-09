using Bureau.Core;
using Bureau.Core.Extensions;
using Bureau.Core.Factories;
using Bureau.Identity.Managers;
using Bureau.Identity.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Identity.Providers
{
    public class BureauIdentityProvider : IIdentityProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserManager _userManager;

        public BureauIdentityProvider(IHttpContextAccessor httpContextAccessor, IUserManager userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }
        public async Task<Result<BureauUser>> GetUserAsync(CancellationToken cancellationToken = default)
        {
            if(_httpContextAccessor.HttpContext == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(_httpContextAccessor.HttpContext));
            }
            Result<IUserId> userIdResult = GetUserId(_httpContextAccessor.HttpContext.User);
            if (userIdResult.IsError) 
            {
                return userIdResult.Error;
            }
            return await _userManager.GetUserByIdAsync(userIdResult.Value);
        }

        public Result<IUserId> GetUserId(ClaimsPrincipal user)
        {
            string? id = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(id))
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(id));
            }
            IUserId userId = BureauUserIdFactory.CreateIBureauUserId(id);
            return new Result<IUserId>(userId);
        }

        public Result<bool> IsUserAuthenticated()
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(_httpContextAccessor.HttpContext));
            }
            if (_httpContextAccessor.HttpContext.User.Identity == null)
            {
                return ErrorMessages.BureauVariableIsNull.Format(nameof(_httpContextAccessor.HttpContext.User.Identity));
            }
            return _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;
        }
    }
}
