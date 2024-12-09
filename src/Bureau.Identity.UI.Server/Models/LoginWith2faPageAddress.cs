using Bureau.Identity.UI.Constants;
using Bureau.UI.Models;

namespace Bureau.Identity.UI.Models
{
    public sealed class LoginWith2faPageAddress : BasePageAddress
    {
        //TODO #12-2
        public string? ReturnUrl { get; set; }
        public bool RememberMe { get; set; } = false;

        public LoginWith2faPageAddress() : base(BureauIdentityUIUris.LoginWith2faAddress)
        {

        }
        public LoginWith2faPageAddress(string? returnUrl, bool rememberMe) : this()
        {
            ReturnUrl = returnUrl;
            RememberMe = rememberMe;
        }

        public override Dictionary<string, object?> QueryParameters
        {
            get
            {
                return new()
                {
                    [BureauIdentityQueryParameterNames.ReturnUrl] = ReturnUrl,
                    [BureauIdentityQueryParameterNames.RememberMe] = RememberMe
                };
            }
        }
    }
}
