using Bureau.Core;
using Bureau.Identity.Constants;

namespace Bureau.Identity.Factories
{
    public class HttpResponseMessageFactory
    {
        public static HttpResponseMessage CreateBureauUserLoginUnauthorizedResponse()
        {
            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            response.Headers.Add(HttpHeaderNames.WWWAuthenticate, HttpMessageOptionNames.BureauUserLoginAuth);
            response.Content = new StringContent(BureauIdentityMessages.BureauUserLoginUnauthorized);
            return response;
        }
        public static HttpResponseMessage CreateBureauUserLoginUnauthorizedResponse(ResultError resultError)
        {
            HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            response.Headers.Add(HttpHeaderNames.WWWAuthenticate, HttpMessageOptionNames.BureauUserLoginAuth);
            response.Content = new StringContent(resultError.ToString());
            return response;
        }
    }
}
