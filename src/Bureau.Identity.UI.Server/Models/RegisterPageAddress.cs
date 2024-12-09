using Bureau.Identity.UI.Constants;
using Bureau.UI.Models;

namespace Bureau.Identity.UI.Models
{
    public sealed class RegisterPageAddress : BasePageAddress
    {
        //TODO #12-2 maybe
        public string ReturnUrl { get; set; } = string.Empty;
        public RegisterPageAddress() : base(BureauIdentityUIUris.Register)
        {
        }

        public RegisterPageAddress(string returnUrl) : this()
        {
            ReturnUrl = returnUrl;
        }

        public override Dictionary<string, object?> QueryParameters
        {
            get
            {
                return new()
                {
                    [BureauIdentityQueryParameterNames.ReturnUrl] = ReturnUrl
                };
            }
        }

    }
}
