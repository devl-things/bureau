using Bureau.UI.Managers;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.UI.Server.Configurations
{
    public static class BureauUIServiceConfiguration
    {
        public static void AddBureauUIManagers(this IServiceCollection services) 
        {
            services.AddScoped<BureauNavigationHistoryManager>();

            services.AddScoped<BureauNavigationManager>();
            services.AddScoped<BureauRedirectManager>();
        }
    }
}
