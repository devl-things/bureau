using Bureau.AspNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Sven.Configurations;
using Sven;
using System.Net;
using System.Text;
using System.Web;

namespace Sven.Controllers
{
    [ApiController]
    [Route(Endpoints.Connect.Base)]
    public partial class ConnectController : BureauApiControllerBase
    {
        public ConnectController(ILogger<ConnectController> logger) : base(logger)
        {
        }

        protected static bool IsResponseType(string actual, string expected)
        {
            return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
        }

        protected IActionResult OAuthError(string error, string? description = null, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return StatusCode((int)statusCode, new OAuthError(error, description));
        }
        protected IActionResult RedirectWithOAuthError(string redirectUri, string error, string? errorDescription, string? state)
        {
            StringBuilder sb = new StringBuilder(redirectUri);
            sb.Append("?");
            sb = RedirectUrlWithOAuthError(sb, error, errorDescription);
            sb = RedirectUrlAppendState(sb, state);
            return Redirect(HttpUtility.UrlEncode(sb.ToString()));
        }
        protected IActionResult RedirectWithOAuthCode(string redirectUri, string code, string? state)
        {
            StringBuilder sb = new StringBuilder(redirectUri);
            sb.Append("?").Append(AuthConstants.OAuth.FieldNames.Code).Append("=").Append(code);
            sb = RedirectUrlAppendState(sb, state);
            return Redirect(sb.ToString());
        }
        protected static StringBuilder RedirectUrlWithOAuthError(StringBuilder redirectUrl, string error, string? errorDescription)
        {
            redirectUrl.Append(AuthConstants.OAuth.FieldNames.Error).Append("=").Append(error);
            if (!string.IsNullOrWhiteSpace(errorDescription))
            {
                redirectUrl.Append("&").Append(AuthConstants.OAuth.FieldNames.ErrorDescription)
                    .Append("=").Append(errorDescription);
            }
            return redirectUrl;
        }
        protected static StringBuilder RedirectUrlAppendState(StringBuilder redirectUrl, string? state)
        {
            if (!string.IsNullOrWhiteSpace(state))
            {
                redirectUrl.Append("&").Append(AuthConstants.OAuth.FieldNames.State)
                    .Append("=").Append(state);
            }

            return redirectUrl;
        }
    }
}