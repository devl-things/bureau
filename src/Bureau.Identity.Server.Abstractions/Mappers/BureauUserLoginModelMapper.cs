using Bureau.Identity.Models;

namespace Bureau.Identity.Mappers
{
    public static class BureauUserLoginModelMapper
    {
        public static GetUserLoginTokensRequest ToGetUserLoginTokensRequest(this BureauUserLoginModel data) 
        {
            return new GetUserLoginTokensRequest(data.UserId, data.LoginProvider, data.ProviderKey);
        }
    }
}
