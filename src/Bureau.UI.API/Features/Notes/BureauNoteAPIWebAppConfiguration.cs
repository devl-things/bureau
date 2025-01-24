using Asp.Versioning.Builder;
using Bureau.UI.API.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Bureau.UI.API.Features.Notes
{
    internal static class BureauNoteAPIWebAppConfiguration
    {
        internal static void MapNotes(this RouteGroupBuilder apiGroupBuilder, ApiVersionSet versionSet)
        {
            RouteGroupBuilder entryRouteBuilder = apiGroupBuilder.MapGroup(BureauAPIRouteNames.NotesGroup);

            entryRouteBuilder.MapPost("text", NotesMethods.CreateTextNote)
                .WithName(BureauAPIRouteNames.CreateTextNote)
                .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);

            entryRouteBuilder.MapGet($"{{id}}", NotesMethods.GetNoteById)
               .WithName(BureauAPIRouteNames.GetTextNoteById)
               .WithApiVersionSet(versionSet)
                .MapToApiVersion(BureauAPIVersion.Version1);
        }
    }
}
