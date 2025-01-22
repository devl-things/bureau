namespace Bureau.Identity.Models
{
    public record struct UpdateUserLoginTokensRequest(GetUserLoginTokensRequest UserLogin, Dictionary<string, string> Tokens);
}
