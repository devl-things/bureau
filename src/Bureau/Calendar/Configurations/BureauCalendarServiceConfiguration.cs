using Bureau.Calendar.Factories;
using Bureau.Calendar.Models;
using Bureau.Calendar.Services;
using Bureau.Core.Models.Data;
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
            services.AddScoped<IDtoProvider<CalendarDto>, CalendarProvider>();
            services.AddScoped<ICommandHandler<CalendarDto>, CalendarCommandHandler>();
            services.AddScoped<IDtoManager<string, CalendarDto>, CalendarManager>();

            return services;
        }
    }
}
