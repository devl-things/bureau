using Bureau.Calendar.Models;
using Bureau.Calendar.Factories;
using Bureau.Calendar.Services;
using Bureau.Factories;
using Bureau.Handlers;
using Bureau.Managers;
using Bureau.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace Bureau.Calendar.Configurations
{
    public static class BureauCalendarServiceConfiguration
    {
        public static IServiceCollection AddBureauCalendar(this IServiceCollection services)
        {
            services.AddScoped<IDtoFactory<CalendarDto>, CalendarDtoFactory>();
            services.AddScoped<IInternalCalendarQueryHandler, CalendarProvider>();
            services.AddScoped<IDtoProvider<string, CalendarDto>, CalendarProvider>();
            services.AddScoped<ICommandHandler<string, CalendarDto>, CalendarCommandHandler>();
            services.AddScoped<IDtoManager<string, CalendarDto>, CalendarManager>();

            services.AddScoped<IDtoProvider<CalendarEventId, CalendarEventDto>, CalendarEventProvider>();
            services.AddScoped<IDtoManager<CalendarEventId, CalendarEventDto>, CalendarEventManager>();
            return services;
        }
    }
}
