using Bureau.Identity.Extensions;
using Bureau.Identity.Models;

namespace Bureau.Identity.Factories
{
    public class HttpRequestMessageFactory
    {
        public static HttpRequestMessage CreateRequestWithBureauUserLogin(HttpMethod method, string uri, BureauUserLoginModel userLogin)
        {
            HttpRequestMessage message = new HttpRequestMessage(method, uri);
            message.EnhanceWithIsAuthenticationNeeded(true);
            message.EnhanceWithBureauUserLogin(userLogin);
            return message;
        }
    }
}
