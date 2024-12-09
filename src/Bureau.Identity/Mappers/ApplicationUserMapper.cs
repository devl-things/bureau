﻿using Bureau.Identity.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Identity.Mappers
{
    internal static class ApplicationUserMapper
    {
        internal static BureauUser ToBureauUser(this ApplicationUser data) 
        {
            return new BureauUser(data.Id, data.UserName!, data.Email);
        }
    }
}
