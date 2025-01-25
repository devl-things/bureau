using Bureau.Calendar.Models;
using Bureau.Core;
using Bureau.Managers;
using Bureau.Providers;
using Bureau.UI.API.Configurations;
using Bureau.UI.API.Features.Calendar.Mappers;
using Bureau.UI.API.Features.Calendar.Models;
using Bureau.UI.API.Models;
using Bureau.UI.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Bureau.UI.API.Features.Calendar
{
    internal static class CalendarMethods
    {
        public static async Task<IResult> CreateCalendar(
            CancellationToken cancellationToken,
            [FromBody] CalendarRequestModel calendar,
            [FromServices] IDtoManager<string, CalendarDto> manager,
            [FromServices] BureauLinkGenerator linkGenerator)
        {
            Result<CalendarDto> result = await manager.InsertAsync(calendar.ToDto(), cancellationToken).ConfigureAwait(false);
            if (result.IsError)
            {
                return Results.BadRequest(result.Error.ToApiResponse());
            }
            // Generate the full URL for the newly created note
            var url = linkGenerator.GetLink(
                $"{BureauAPIRouteNames.GetCalendarById}-{BureauAPIVersion.Version1}", // Name of the route we want to link to
                new RouteValueDictionary
                {
                    { "id", result.Value.Id }
                }
            );
            ApiResponse<CalendarResponseModel> response = result.ToApiResponse(x => x.ToResponseModel());
            // Return Created result with the generated URL
            return Results.Created(url, response);
        }
        public static async Task<IResult> UpdateCalendar(
            string id,
            CancellationToken cancellationToken,
            [FromBody] CalendarRequestModel calendar,
            [FromServices] IDtoManager<string, CalendarDto> manager)
        {
            Result<CalendarDto> result = await manager.UpdateAsync(calendar.ToDto(id), cancellationToken).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return Results.Ok(result.ToApiResponse(x => x.ToResponseModel()));
            }
            return Results.BadRequest(result.Error.ToApiResponse());
        }
        public static async Task<IResult> GetCalendarById(string id, CancellationToken cancellationToken, [FromServices] IDtoProvider<CalendarDto> provider)
        {
            Result<CalendarDto> result = await provider.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                return Results.Ok(result.ToApiResponse(x => x.ToResponseModel()));
            }
            return Results.BadRequest(result.Error.ToApiResponse());
        }

        public static async Task<IResult> GetCalendars(CancellationToken cancellationToken, [FromServices] IDtoProvider<CalendarDto> provider, [FromQuery] int? page, [FromQuery] int? limit)
        {
            PaginatedResult<List<CalendarDto>> values = await provider.GetAsync(page, limit, cancellationToken).ConfigureAwait(false);
            if (values.IsSuccess)
            {
                return Results.Ok(values.ToPagedApiResponse(x => x.ToResponseModel()));
            }
            return Results.BadRequest(values.Error.ToApiResponse());
        }

        internal static async Task<IResult> DeleteCalendar(string id, CancellationToken cancellationToken, [FromServices] IDtoManager<string, CalendarDto> manager)
        {
            Result response = await manager.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            if (response.IsError)
            {
                return Results.BadRequest(response.Error.ToApiResponse());
            }
            return Results.Ok(response.ToApiResponse("Deletion succeeded."));
        }
    }
}
