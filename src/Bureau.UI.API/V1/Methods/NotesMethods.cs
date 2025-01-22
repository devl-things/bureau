using Bureau.UI.API.V1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bureau.UI.API.V1.Methods
{
    internal static class NotesMethods
    {

        public static IResult CreateTextNote(TextNoteModel note, LinkGenerator linkGenerator, HttpContext httpContext)
        {
            // Generate the full URL for the newly created note
            var url = linkGenerator.GetUriByAddress(
                httpContext,
                BureauAPIRouteNames.GetTextNoteById, // Name of the route we want to link to
                new RouteValueDictionary
                {
                    { "id", note.Id }
                }
            );

            // Return Created result with the generated URL
            return Results.Created(url, note);
        }

        // Handler method for the GET request
        public static IResult GetNoteById(string id)
        {
            // Dummy data for demonstration; you’d retrieve this from a database
            var note = new TextNoteModel { Id = id };
            return Results.Ok(note);
        }
    }
}
