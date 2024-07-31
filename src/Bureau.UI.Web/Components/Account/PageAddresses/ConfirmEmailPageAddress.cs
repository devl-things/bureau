using Bureau.UI.Web.Components.Helpers;

namespace Bureau.UI.Web.Components.Account.PageAddresses
{
    internal class ConfirmEmailPageAddress : BasePageAddress
    {
        //TODO #12-2
        public string? ReturnUrl { get; set; }
        public string? UserId { get; set; }
        public string? Code { get; set; }
        public ConfirmEmailPageAddress() : base(UriPages.ConfirmEmail)
        {
        }

        public ConfirmEmailPageAddress(string? userId, string? code) : this()
        {
            UserId = userId;
            Code = code;
        }
        public ConfirmEmailPageAddress(string? userId, string? code, string? returnUrl) : this(userId, code)
        {
            ReturnUrl = returnUrl;
        }

        internal override Dictionary<string, object?> QueryParameters
        {
            get
            {
                Dictionary<string, object?> dict = new()
                {
                    [QueryParameterNames.UserId] = UserId,
                    [QueryParameterNames.Code] = Code,
                };
                if(string.IsNullOrWhiteSpace(ReturnUrl)) 
                { 
                    dict.Add(QueryParameterNames.ReturnUrl, ReturnUrl); 
                }
                return dict;
            }
        }

    }
}