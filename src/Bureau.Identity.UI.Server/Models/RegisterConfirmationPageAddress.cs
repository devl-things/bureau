using Bureau.Identity.UI.Constants;
using Bureau.UI.Models;

namespace Bureau.Identity.UI.Models
{
    public class RegisterConfirmationPageAddress : BasePageAddress
    {
        //TODO #12-2
        public string? ReturnUrl { get; set; }
        public string? UserName { get; set; }
        public RegisterConfirmationPageAddress() : base(BureauIdentityUIUris.RegisterConfirmation)
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

        public override Dictionary<string, object?> QueryParameters
        {
            get
            {
                Dictionary<string, object?> dict = new()
                {
                    [BureauIdentityQueryParameterNames.UserName] = UserName
                };
                if (string.IsNullOrWhiteSpace(ReturnUrl))
                {
                    dict.Add(BureauIdentityQueryParameterNames.ReturnUrl, ReturnUrl);
                }
                return dict;
            }
        }

    }
}
