using Bureau.Google.Calendar.Managers;
using Bureau.Google.Calendar.Services;
using Bureau.Google.Calendar.Services.Cache;
using Bureau.Google.Configurations;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.Google.Calendar.Configurations
{
    public static class BureauGoogleCalendarServiceConfiguration
    {
        public static void AddGoogleCalendarModule(this IServiceCollection services, Action<GoogleOptions> configureOptions) 
        {
            services.AddGoogleApiHttpClient(configureOptions);
            services.AddScoped<ICalendarManager, CalendarManager>();
            services.AddScoped<ICalendarListService, CalendarListService>();
            services.Decorate<ICalendarListService, CalendarListServiceCache>();
        }
    }
}
