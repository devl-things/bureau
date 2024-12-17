using Bureau.UI.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.UI.Server.API.Methods
{
    internal static class NotesMethod
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
