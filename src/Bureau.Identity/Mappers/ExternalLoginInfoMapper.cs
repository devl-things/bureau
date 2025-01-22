using Bureau.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Identity.Mappers
{
    internal static class ExternalLoginInfoMapper
    {
        internal static BureauExternalLogin ToBureauExternalLogin(this ExternalLoginInfo data) 
        {
            return new BureauExternalLogin()
            {
                LoginProvider = data.LoginProvider,
                ProviderKey = data.ProviderKey,
                ProviderDisplayName = data.ProviderDisplayName,
                Principal = data.Principal,
                AuthenticationProperties = data.AuthenticationProperties,
                AuthenticationTokens = data.AuthenticationTokens,
            };
        }
    }
}
