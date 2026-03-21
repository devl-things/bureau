using Bureau;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Extensions;
using Sven;
using Sven.Services;
using System.Security.Claims;

namespace Sven.PageModels.Connect.SignIn
{
    public class TicketSignInHandler : ISignInGetHandler
    {
        private readonly ILogger<TicketSignInHandler> _logger;
        private readonly IUserService _userService;
        private readonly IUserClaimsProvider _userClaimsProvider;

        public TicketSignInHandler(ILogger<TicketSignInHandler> logger, IUserService userService, IUserClaimsProvider userClaimsProvider)
        {
            _logger = logger;
            _userService = userService;
            _userClaimsProvider = userClaimsProvider;
        }

        public async Task<IActionResult> HandleGetAsync(SignInContext context, CancellationToken cancellationToken = default)
        {
            string? ticket = context.Request.GetQueryStringParameter(AuthConstants.PropertyNames.Ticket);
            if (string.IsNullOrWhiteSpace(ticket))
            {
                return context.Redirect(Endpoints.Connect.SignIn);
            }

            Result<string> userIdResult = await _userService.GetUserIdByTicketAsync(ticket, cancellationToken);
            if (userIdResult.IsError)
            {
                _logger.LogResultError(userIdResult.Error);
                return context.Redirect(Endpoints.Connect.SignIn);
            }

            Result<ClaimsPrincipal> claimsPrincipalResult = await _userClaimsProvider.GetClaimsPrincipalAsync(userIdResult.Value, cancellationToken);
            if (claimsPrincipalResult.IsError)
            {
                _logger.LogResultError(claimsPrincipalResult.Error);
                return context.Redirect(Endpoints.Connect.SignIn);
            }

            await context.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipalResult.Value);
            return context.Redirect(Endpoints.Account.AccountInfo);
        }
    }
}
