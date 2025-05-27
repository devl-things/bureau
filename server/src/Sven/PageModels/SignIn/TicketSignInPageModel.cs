using Bureau.Core;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven.Models;
using Sven.Services;
using System.Security.Claims;

namespace Sven.PageModels.SignIn
{
    public class TicketSignInPageModel : SignInPageModel
    {
        private readonly IUserProvider _userProvider;
        public TicketSignInPageModel(ILogger<PlainSignInPageModel> logger, IUserProvider userProvider, IUserClaimsProvider userClaimProvider) : base(logger, userClaimProvider)
        {
            _userProvider = userProvider;
        }
        public async override Task<IActionResult> HandleGetRequestAsync(CancellationToken cancellationToken = default)
        {
            string? ticket = GetQueryStringParameter(AuthConstants.PropertyNames.Ticket);
            if (string.IsNullOrWhiteSpace(ticket))
            {
                return Redirect(Endpoints.Connect.SignIn);
            }
            Result<string> userIdResult = await _userProvider.GetUserIdByTicketAsync(ticket, cancellationToken);
            if (userIdResult.IsError)
            {
                _logger.LogResultError(userIdResult.Error);
                return Redirect(Endpoints.Connect.SignIn);
            }

            Result<ClaimsPrincipal> claimsPrincipalResult = await _userClaimsProvider.GetClaimsPrincipalAsync(userIdResult.Value, cancellationToken);

            if (claimsPrincipalResult.IsError)
            {
                _logger.LogResultError(claimsPrincipalResult.Error);
                // TODO have a error message pass to redirect
                ErrorMessage = AuthConstants.OAuth.ErrorDescriptions.InvalidUsernamePassword;
                return Redirect(Endpoints.Connect.SignIn);
            }
            await SignInUserAsync(claimsPrincipalResult.Value, cancellationToken);

            return Redirect(Endpoints.Account.AccountInfo);
        }

        public override Task<IActionResult> HandleLoginAsync(LoginCredentials credentials, CancellationToken cancellationToken = default)
        {
            _logger.LogResultError(new ResultError("Something called post method with ticket mode"));
            return Task.FromResult<IActionResult>(StatusCode(StatusCodes.Status405MethodNotAllowed));
        }
    }
}
