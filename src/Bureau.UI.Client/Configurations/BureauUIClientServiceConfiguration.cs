using Bureau.UI.Client.Events;
using Bureau.UI.Client.Managers;
using Bureau.UI.Events;
using Bureau.UI.Managers;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.UI.Client.Configurations
{
    public static class BureauUIClientServiceConfiguration
    {
        public static void AddBureauUIClient(this IServiceCollection services)
        {
            //This should be registered as Scoped on Blazor server
            services.AddSingleton<BureauNavigationHistoryManager>();

            services.AddScoped<ITagManager, TagManager>();
            services.AddScoped<IEntryManager, EntryManager>();

            services.AddSingleton<IEventMessenger<HeaderNavigationEvent>, EventMessenger<HeaderNavigationEvent>>();
        }
    }
}
