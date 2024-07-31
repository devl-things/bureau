using Bureau.UI.Web.Components.Helpers;

namespace Bureau.UI.Web.Components.Account.PageAddresses
{
    internal sealed class RegisterPageAddress : BasePageAddress
    {
        //TODO #12-2 maybe
        public string ReturnUrl { get; set; } = string.Empty;
        public RegisterPageAddress() : base(UriPages.Register)
        {
        }

        public RegisterPageAddress(string returnUrl) : this()
        {
            ReturnUrl = returnUrl;
        }

        internal override Dictionary<string, object?> QueryParameters
        {
            get
            {
                return new()
                {
                    [QueryParameterNames.ReturnUrl] = ReturnUrl
                };
            }
        }

    }
}
