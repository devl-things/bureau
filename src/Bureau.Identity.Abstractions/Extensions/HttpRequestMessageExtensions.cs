using Bureau.Identity.Constants;
using Bureau.Identity.Models;

namespace Bureau.Identity.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static void EnhanceWithBureauUserLogin(this HttpRequestMessage message, BureauUserLoginModel userLogin)
        {
            message.Options.Set(new HttpRequestOptionsKey<BureauUserLoginModel>(HttpMessageOptionNames.BureauUserLoginAuth), userLogin);
        }

        public static bool TryGetBureauUserLogin(this HttpRequestMessage message, out BureauUserLoginModel? userLogin)
        {
            return message.Options.TryGetValue(new HttpRequestOptionsKey<BureauUserLoginModel>(HttpMessageOptionNames.BureauUserLoginAuth), out userLogin);
        }

        public static void EnhanceWithIsAuthenticationNeeded(this HttpRequestMessage message, bool isAuthenticationNeeded)
        {
            message.Options.Set(new HttpRequestOptionsKey<bool>(HttpMessageOptionNames.IsAuthenticationNeeded), isAuthenticationNeeded);
        }

        public static bool GetIsAuthenticationNeeded(this HttpRequestMessage message)
        {
            if (message.Options.TryGetValue(new HttpRequestOptionsKey<bool>(HttpMessageOptionNames.IsAuthenticationNeeded), out bool isAuthenticationNeeded))
            {
                return isAuthenticationNeeded;
            }
            return false;
        }
    }
}
