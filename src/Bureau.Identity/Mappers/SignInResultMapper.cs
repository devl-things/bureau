using Bureau.Identity.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Identity.Mappers
{
    internal static class SignInResultMapper
    {
        internal static BureauSignInResult ToBureauSignInResult(this SignInResult data) 
        {
            return new BureauSignInResult()
            {
                IsSuccess = data.Succeeded,
                IsLockedOut = data.IsLockedOut,
                IsNotAllowed = data.IsNotAllowed,
                RequiresTwoFactor = data.RequiresTwoFactor,
            };
        }
    }
}
