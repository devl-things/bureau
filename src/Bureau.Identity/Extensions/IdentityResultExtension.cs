using Bureau.Core;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Identity.Extensions
{
    internal static class IdentityResultExtension
    {
        private static string ToErrorString(this IdentityResult data) 
        {
            return string.Join(", ", data.Errors.Select(x => $"{x.Code} - {x.Description}"));
        }
        internal static Result ToResult(this IdentityResult data) 
        {
            return new Result(new ResultError(data.ToErrorString()));
        }

        internal static ResultError ToResultError(this IdentityResult data)
        {
            return new ResultError(data.ToErrorString());
        }
    }
}
