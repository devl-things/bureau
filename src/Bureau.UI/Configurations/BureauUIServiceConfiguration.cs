using Bureau.UI.Accessors;
using Bureau.UI.Managers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.UI.Configurations
{
    public static class BureauUIServiceConfiguration
    {
        public static void AddBureauUI(this IServiceCollection services) 
        {
            services.AddSingleton<BureauNavigationManager>();
            services.AddScoped<IUserAccessor, UserAccessor>();
        }
    }
}
