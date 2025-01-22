using Bureau.Identity.UI.Constants;
using Bureau.UI.Models;

namespace Bureau.Identity.UI.Models
{
    public class ConfirmEmailPageAddress : BasePageAddress
    {
        //TODO #12-2
        public string? ReturnUrl { get; set; }
        public string? UserId { get; set; }
        public string? Code { get; set; }
        public ConfirmEmailPageAddress() : base(BureauIdentityUIUris.ConfirmEmail)
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

        public override Dictionary<string, object?> QueryParameters
        {
            get
            {
                Dictionary<string, object?> dict = new()
                {
                    [BureauIdentityQueryParameterNames.UserId] = UserId,
                    [BureauIdentityQueryParameterNames.Code] = Code,
                };
                if(string.IsNullOrWhiteSpace(ReturnUrl)) 
                { 
                    dict.Add(BureauIdentityQueryParameterNames.ReturnUrl, ReturnUrl); 
                }
                return dict;
            }
        }

    }
}