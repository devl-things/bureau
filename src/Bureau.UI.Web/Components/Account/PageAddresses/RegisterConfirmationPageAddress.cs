using Bureau.UI.Web.Components.Helpers;

namespace Bureau.UI.Web.Components.Account.PageAddresses
{
    internal class RegisterConfirmationPageAddress : BasePageAddress
    {
        //TODO #12-2
        public string? ReturnUrl { get; set; }
        public string? UserName { get; set; }
        public RegisterConfirmationPageAddress() : base(UriPages.RegisterConfirmation)
        {
        }
        public RegisterConfirmationPageAddress(string? userName) : this()
        {
            UserName = userName;
        }

        public RegisterConfirmationPageAddress(string? userName, string? returnUrl) : this(userName)
        {
            ReturnUrl = returnUrl;
        }

        internal override Dictionary<string, object?> QueryParameters
        {
            get
            {
                Dictionary<string, object?> dict = new()
                {
                    [QueryParameterNames.UserName] = UserName
                };
                if (string.IsNullOrWhiteSpace(ReturnUrl))
                {
                    dict.Add(QueryParameterNames.ReturnUrl, ReturnUrl);
                }
                return dict;
            }
        }

    }
}
