using Bureau.UI.Web.Components.Helpers;

namespace Bureau.UI.Web.Components.Account.PageAddresses
{
    internal sealed class LoginWith2faPageAddress : BasePageAddress
    {
        //TODO #12-2
        public string? ReturnUrl { get; set; }
        public bool RememberMe { get; set; } = false;

        public LoginWith2faPageAddress() : base(UriPages.LoginWith2faAddress)
        {

        }
        public LoginWith2faPageAddress(string? returnUrl, bool rememberMe) : this()
        {
            ReturnUrl = returnUrl;
            RememberMe = rememberMe;
        }

        internal override Dictionary<string, object?> QueryParameters
        {
            get
            {
                return new()
                {
                    [QueryParameterNames.ReturnUrl] = ReturnUrl,
                    [QueryParameterNames.RememberMe] = RememberMe
                };
            }
        }
    }
}
