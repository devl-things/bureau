using Asp.Versioning.Builder;
using Bureau.UI.API.Configurations;
using Bureau.UI.API.Filters;
using Bureau.UI.API.V1.Methods;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bureau.UI.API.V1.Configurations
{
    internal static partial class BureauCalendarAPIWebAppConfiguration
    {
        internal static void MapCalendar(this RouteGroupBuilder apiGroupBuilder, ApiVersionSet versionSet)
        {
            RouteGroupBuilder group = apiGroupBuilder.MapGroup(BureauAPIRouteNames.CalendarsGroup);

            group.MapDelete("{id}", CalendarMethods.DeleteCalendar)
                .WithName($"{BureauAPIRouteNames.DeleteCalendar}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);

            group.MapGet("", CalendarMethods.GetCalendars)
                .AddEndpointFilter<PaginationValidationFilter>()
                .WithName($"{BureauAPIRouteNames.GetCalendars}-{BureauAPIVersion.Version1}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);

            group.MapGet("{id}", CalendarMethods.GetCalendarById)
                .WithName($"{BureauAPIRouteNames.GetCalendarById}-{BureauAPIVersion.Version1}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);

            group.MapPost("", CalendarMethods.CreateCalendar)
                .WithName($"{BureauAPIRouteNames.CreateCalendar}-{BureauAPIVersion.Version1}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);

            group.MapPut("{id}", CalendarMethods.UpdateCalendar)
                .WithName($"{BureauAPIRouteNames.UpdateCalendar}-{BureauAPIVersion.Version1}")
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);
        }
    }
}
