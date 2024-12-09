namespace Bureau.Identity.Models
{
    public record struct GetUserLoginTokensRequest(string UserId, string LoginProvider, string ProviderKey);
}
